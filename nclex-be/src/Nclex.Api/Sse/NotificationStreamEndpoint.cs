using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using Nclex.Application.Abstractions;
using Nclex.Application.Common;
using Nclex.Application.Notifications;
using Nclex.Application.Notifications.ReadState;

namespace Nclex.Api.Sse;

/// <summary>
/// GET /api/v1/notifications/stream?ticket=…: the bell's live feed as Server-Sent Events
/// (api.md section 6, architecture.md 5.6). Sends <c>unread_count</c> on connect, then
/// <c>notification</c> and <c>unread_count</c> as they happen on any replica (via Redis), and
/// <c>ping</c> every 25 seconds so proxies keep the connection open.
/// </summary>
/// <remarks>
/// Authenticated by a single-use ticket instead of a bearer token, because EventSource cannot
/// send headers. Not in the OpenAPI document: the frontend opens it with EventSource, not the
/// generated client.
/// </remarks>
public static class NotificationStreamEndpoint
{
    public const string Path = "/api/v1/notifications/stream";

    public static readonly TimeSpan PingInterval = TimeSpan.FromSeconds(25);

    public static IEndpointConventionBuilder MapNotificationStream(this IEndpointRouteBuilder app) =>
        app.MapGet(Path, StreamAsync).AllowAnonymous().ExcludeFromDescription();

    private static async Task StreamAsync(
        string? ticket,
        HttpContext http,
        IStreamTicketStore tickets,
        INotificationBus bus,
        UnreadCountHandler unreadCount,
        IOptions<JsonOptions> json)
    {
        var cancellationToken = http.RequestAborted;
        var userId = ticket is { Length: > 0 } ? await tickets.RedeemAsync(ticket, cancellationToken) : null;
        if (userId is not { } user)
        {
            throw new AppException(NotificationErrors.InvalidTicket);
        }

        StartEventStream(http);

        // One queue for everything written to the response, so writes never overlap.
        var events = Channel.CreateUnbounded<StreamEvent>(new UnboundedChannelOptions { SingleReader = true });
        await using var subscription = await bus.SubscribeAsync(
            user,
            message =>
            {
                events.Writer.TryWrite(new StreamEvent(message));
                return Task.CompletedTask;
            },
            cancellationToken);
        _ = PingAsync(events.Writer, cancellationToken);

        var serializer = json.Value.SerializerOptions;
        try
        {
            await WriteAsync(http.Response, "unread_count", await unreadCount.HandleAsync(user, cancellationToken), serializer, cancellationToken);
            await foreach (var next in events.Reader.ReadAllAsync(cancellationToken))
            {
                if (next.Message is null)
                {
                    await WriteAsync(http.Response, "ping", new { }, serializer, cancellationToken);
                    continue;
                }

                if (next.Message.Notification is { } notification)
                {
                    await WriteAsync(http.Response, "notification", notification, serializer, cancellationToken);
                }

                await WriteAsync(http.Response, "unread_count", await unreadCount.HandleAsync(user, cancellationToken), serializer, cancellationToken);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // The client went away; disposing the subscription above unsubscribes from Redis.
        }
    }

    private static void StartEventStream(HttpContext http)
    {
        http.Response.ContentType = "text/event-stream";
        http.Response.Headers.CacheControl = "no-cache";
        // Tells nginx-style proxies (and the Next.js rewrite) not to hold events back.
        http.Response.Headers["X-Accel-Buffering"] = "no";
        http.Features.Get<IHttpResponseBodyFeature>()?.DisableBuffering();
    }

    private static async Task PingAsync(ChannelWriter<StreamEvent> events, CancellationToken cancellationToken)
    {
        using var timer = new PeriodicTimer(PingInterval);
        try
        {
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                events.TryWrite(StreamEvent.Ping);
            }
        }
        catch (OperationCanceledException)
        {
            // Stream closed.
        }
    }

    private static async Task WriteAsync<T>(
        HttpResponse response,
        string eventName,
        T data,
        JsonSerializerOptions serializer,
        CancellationToken cancellationToken)
    {
        var frame = $"event: {eventName}\ndata: {JsonSerializer.Serialize(data, serializer)}\n\n";
        await response.Body.WriteAsync(Encoding.UTF8.GetBytes(frame), cancellationToken);
        await response.Body.FlushAsync(cancellationToken);
    }

    /// <summary>A bus message to forward, or a keep-alive ping when <see cref="Message"/> is null.</summary>
    private sealed record StreamEvent(NotificationBusMessage? Message)
    {
        public static readonly StreamEvent Ping = new((NotificationBusMessage?)null);
    }
}
