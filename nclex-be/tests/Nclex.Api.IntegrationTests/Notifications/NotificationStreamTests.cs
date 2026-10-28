using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Nclex.Api.IntegrationTests.Infrastructure;
using Nclex.Application.Abstractions;
using Nclex.Domain.Notifications;
using Nclex.Infrastructure.Persistence;

namespace Nclex.Api.IntegrationTests.Notifications;

/// <summary>
/// The SSE stream on real Redis (architecture.md 5.6). Two hosts on the same Postgres and Redis
/// stand in for two API replicas: whatever happens on one must reach a stream held by the other.
/// </summary>
public sealed class NotificationStreamTests(ApiFactory factory) : IAsyncDisposable
{
    private readonly RedisTestHost _replicaA = new(factory);
    private readonly RedisTestHost _replicaB = new(factory);

    [Fact]
    public async Task A_ticket_opens_the_stream_once()
    {
        var user = await _replicaA.Auth.SignupNewUserAsync();
        var ticket = await _replicaA.Notifications.StreamTicketOkAsync(user.AccessToken);

        using (var stream = await _replicaA.Notifications.OpenStreamAsync(ticket))
        {
            stream.StatusCode.ShouldBe(HttpStatusCode.OK);
            stream.Content.Headers.ContentType!.MediaType.ShouldBe("text/event-stream");
            using var events = new SseReader(await stream.Content.ReadAsStreamAsync(TestContext.Current.CancellationToken));
            var first = await events.NextAsync();
            (first.Event, first.Data.GetProperty("count").GetInt32()).ShouldBe(("unread_count", 0));
        }

        await (await _replicaB.Notifications.OpenStreamAsync(ticket)).ShouldBeProblemAsync(HttpStatusCode.Unauthorized, "UNAUTHENTICATED");
    }

    [Fact]
    public async Task A_made_up_ticket_is_refused()
    {
        await (await _replicaA.Notifications.OpenStreamAsync("not-a-ticket")).ShouldBeProblemAsync(HttpStatusCode.Unauthorized, "UNAUTHENTICATED");
    }

    [Fact]
    public async Task A_result_graded_on_another_replica_reaches_the_open_stream()
    {
        var exam = await new ExamScenario(factory, _replicaB.Attempts).StartAsync(new { revealMode = "exam", passThreshold = 0.5 });
        using var events = await OpenStreamAsync(_replicaA, exam.Token);

        await new ExamScenario(factory, _replicaB.Attempts).SubmitOkAsync(exam);

        var notification = await events.NextNonPingAsync();
        notification.Event.ShouldBe("notification");
        notification.Data.GetProperty("type").GetString().ShouldBe("result_fail");
        notification.Data.GetProperty("relatedAttemptId").GetGuid().ShouldBe(exam.AttemptId);
        var count = await events.NextNonPingAsync();
        (count.Event, count.Data.GetProperty("count").GetInt32()).ShouldBe(("unread_count", 1));
    }

    [Fact]
    public async Task Reading_on_another_replica_updates_the_badge()
    {
        var user = await _replicaB.Auth.SignupNewUserAsync();
        await _replicaB.Auth.SendWithTokenAsync(
            HttpMethod.Post,
            "/api/v1/me/password",
            user.AccessToken,
            new { currentPassword = AuthApi.Password, newPassword = "a-brand-new-password" });
        using var events = await OpenStreamAsync(_replicaA, user.AccessToken, expectedUnread: 1);

        await _replicaB.Notifications.MarkAllReadAsync(user.AccessToken);

        var count = await events.NextNonPingAsync();
        (count.Event, count.Data.GetProperty("count").GetInt32()).ShouldBe(("unread_count", 0));
    }

    [Fact]
    public async Task A_rolled_back_notification_is_never_pushed()
    {
        var user = await _replicaA.Auth.SignupNewUserAsync();
        using var events = await OpenStreamAsync(_replicaA, user.AccessToken);

        await using (var scope = _replicaB.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await using var transaction = await db.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);
            await scope.ServiceProvider.GetRequiredService<INotificationPublisher>().PublishAsync(
                new NewNotification(
                    user.Body.User.Id,
                    NotificationType.System,
                    NotificationCategory.Engagement,
                    new NotificationText("Rolled back", "Never committed"),
                    new { }),
                TestContext.Current.CancellationToken);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            await transaction.RollbackAsync(TestContext.Current.CancellationToken);
        }

        // A committed notice afterwards must be the first thing the stream sees.
        await _replicaB.Auth.SendWithTokenAsync(
            HttpMethod.Post,
            "/api/v1/me/password",
            user.AccessToken,
            new { currentPassword = AuthApi.Password, newPassword = "a-brand-new-password" });

        var next = await events.NextNonPingAsync();
        next.Data.GetProperty("title").GetString().ShouldBe("Your password was changed");
    }

    public async ValueTask DisposeAsync()
    {
        await _replicaA.DisposeAsync();
        await _replicaB.DisposeAsync();
    }

    /// <summary>Opens a stream on <paramref name="replica"/> and reads past its opening unread count.</summary>
    private static async Task<SseReader> OpenStreamAsync(RedisTestHost replica, string token, int expectedUnread = 0)
    {
        var ticket = await replica.Notifications.StreamTicketOkAsync(token);
        var response = await replica.Notifications.OpenStreamAsync(ticket);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var events = new SseReader(await response.Content.ReadAsStreamAsync(TestContext.Current.CancellationToken));
        var first = await events.NextAsync();
        (first.Event, first.Data.GetProperty("count").GetInt32()).ShouldBe(("unread_count", expectedUnread));
        return events;
    }
}
