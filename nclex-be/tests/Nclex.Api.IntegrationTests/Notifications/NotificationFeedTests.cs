using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Nclex.Api.IntegrationTests.Infrastructure;
using Nclex.Domain.Notifications;
using Nclex.Infrastructure.Persistence;

namespace Nclex.Api.IntegrationTests.Notifications;

/// <summary>The feed, the unread count and read state (api.md section 6).</summary>
public sealed class NotificationFeedTests(ApiFactory factory)
{
    private static readonly DateTimeOffset Start = new(2026, 10, 1, 9, 0, 0, TimeSpan.Zero);

    private readonly AuthApi _auth = new(factory);
    private readonly NotificationsApi _notifications = new(new AuthApi(factory));

    [Fact]
    public async Task The_feed_pages_newest_first()
    {
        var user = await _auth.SignupNewUserAsync();
        var ids = await AddAsync(user.Body.User.Id, count: 3);

        var first = await _notifications.ListOkAsync(user.AccessToken, "?limit=2");
        var second = await _notifications.ListOkAsync(user.AccessToken, $"?limit=2&cursor={first.GetProperty("nextCursor").GetString()}");

        NotificationsApi.Ids(first).ShouldBe([ids[2], ids[1]]);
        NotificationsApi.Ids(second).ShouldBe([ids[0]]);
        second.GetProperty("nextCursor").ValueKind.ShouldBe(System.Text.Json.JsonValueKind.Null);
    }

    [Fact]
    public async Task Marking_one_read_drops_the_count_and_hides_it_from_unread_only()
    {
        var user = await _auth.SignupNewUserAsync();
        var ids = await AddAsync(user.Body.User.Id, count: 3);

        (await _notifications.MarkReadAsync(user.AccessToken, ids[1])).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await _notifications.MarkReadAsync(user.AccessToken, ids[1])).StatusCode.ShouldBe(HttpStatusCode.NoContent);

        (await _notifications.UnreadCountAsync(user.AccessToken)).ShouldBe(2);
        NotificationsApi.Ids(await _notifications.ListOkAsync(user.AccessToken, "?unreadOnly=true")).ShouldBe([ids[2], ids[0]]);
        var read = (await _notifications.ListOkAsync(user.AccessToken)).GetProperty("items")[1];
        read.GetProperty("readAt").ValueKind.ShouldBe(System.Text.Json.JsonValueKind.String);
    }

    [Fact]
    public async Task Marking_all_read_clears_the_badge()
    {
        var user = await _auth.SignupNewUserAsync();
        await AddAsync(user.Body.User.Id, count: 3);

        (await _notifications.MarkAllReadAsync(user.AccessToken)).StatusCode.ShouldBe(HttpStatusCode.NoContent);

        (await _notifications.UnreadCountAsync(user.AccessToken)).ShouldBe(0);
    }

    [Fact]
    public async Task Another_users_notification_cannot_be_seen_or_marked()
    {
        var owner = await _auth.SignupNewUserAsync();
        var other = await _auth.SignupNewUserAsync();
        var ids = await AddAsync(owner.Body.User.Id, count: 1);

        await (await _notifications.MarkReadAsync(other.AccessToken, ids[0])).ShouldBeProblemAsync(HttpStatusCode.NotFound, "NOT_FOUND");
        (await _notifications.ListOkAsync(other.AccessToken)).GetProperty("items").GetArrayLength().ShouldBe(0);
        (await _notifications.UnreadCountAsync(owner.AccessToken)).ShouldBe(1);
    }

    [Theory]
    [InlineData("?cursor=nonsense")]
    [InlineData("?limit=0")]
    [InlineData("?limit=51")]
    public async Task A_bad_query_is_rejected(string query)
    {
        var user = await _auth.SignupNewUserAsync();

        await (await _notifications.ListAsync(user.AccessToken, query)).ShouldBeProblemAsync(HttpStatusCode.BadRequest, "VALIDATION_FAILED");
    }

    [Fact]
    public async Task The_feed_needs_a_signed_in_user()
    {
        await (await _notifications.ListAsync(token: null!)).ShouldBeProblemAsync(HttpStatusCode.Unauthorized, "UNAUTHENTICATED");
    }

    /// <summary>Adds in-app notifications a minute apart, oldest first, and returns their ids.</summary>
    private async Task<List<Guid>> AddAsync(Guid userId, int count)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var rows = Enumerable.Range(0, count).Select(i => Notification.Create(
            userId,
            NotificationType.System,
            NotificationCategory.Engagement,
            $"Notice {i}",
            "Body",
            "{}",
            relatedAttemptId: null,
            [NotificationChannels.InApp],
            Start.AddMinutes(i))).ToList();
        db.Notifications.AddRange(rows);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        return [.. rows.Select(r => r.Id)];
    }
}
