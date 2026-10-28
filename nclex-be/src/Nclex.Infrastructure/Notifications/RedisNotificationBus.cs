using System.Text.Json;
using Microsoft.Extensions.Logging;
using Nclex.Application.Abstractions;
using StackExchange.Redis;

namespace Nclex.Infrastructure.Notifications;

/// <summary>
/// Redis pub/sub on <c>user:{id}:notifications</c> (architecture.md 5.6). Publishing fails open:
/// with Redis down the notification is still in Postgres, and clients poll the unread count.
/// </summary>
internal sealed partial class RedisNotificationBus(
    IConnectionMultiplexer redis,
    ILogger<RedisNotificationBus> logger) : INotificationBus
{
    private static readonly JsonSerializerOptions MessageJson = new(JsonSerializerDefaults.Web);

    public static RedisChannel ChannelFor(Guid userId) => RedisChannel.Literal($"user:{userId}:notifications");

    public async Task PublishAsync(Guid userId, NotificationBusMessage message, CancellationToken cancellationToken)
    {
        // Checked first so an outage costs nothing per request, instead of a timeout each time.
        if (!redis.IsConnected)
        {
            LogRedisUnavailable(logger);
            return;
        }

        try
        {
            await redis.GetSubscriber().PublishAsync(ChannelFor(userId), JsonSerializer.Serialize(message, MessageJson));
        }
        catch (RedisException ex)
        {
            LogPublishFailed(logger, ex);
        }
    }

    public async Task<IAsyncDisposable> SubscribeAsync(
        Guid userId,
        Func<NotificationBusMessage, Task> onMessage,
        CancellationToken cancellationToken)
    {
        // Each subscription gets its own queue, so two tabs of one user on this replica both hear every message.
        var queue = await redis.GetSubscriber().SubscribeAsync(ChannelFor(userId));
        queue.OnMessage(async message =>
        {
            if (JsonSerializer.Deserialize<NotificationBusMessage>(message.Message.ToString(), MessageJson) is { } parsed)
            {
                await onMessage(parsed);
            }
        });
        return new Subscription(queue);
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Redis is unavailable; notification not pushed, clients will poll")]
    private static partial void LogRedisUnavailable(ILogger logger);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Publishing a notification to Redis failed; clients will poll")]
    private static partial void LogPublishFailed(ILogger logger, Exception exception);

    private sealed class Subscription(ChannelMessageQueue queue) : IAsyncDisposable
    {
        public async ValueTask DisposeAsync() => await queue.UnsubscribeAsync();
    }
}
