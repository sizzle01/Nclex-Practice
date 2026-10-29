using System.Net;
using System.Text.Json;
using Nclex.Api.IntegrationTests.Infrastructure;
using Nclex.Domain.Entities;
using Nclex.Domain.Notifications;

namespace Nclex.Api.IntegrationTests.Flags;

/// <summary>The moderation queue and PATCH /admin/flags/{id} (api.md section 8).</summary>
public sealed class ModerationTests(ApiFactory factory)
{
    private readonly AuthApi _auth = new(factory);
    private readonly FlagsApi _flags = new(factory, new AuthApi(factory));
    private readonly FlagScenario _scenario = new(factory);
    private readonly NotificationProbe _notifications = new(factory);
    private readonly AuditProbe _audit = new(factory);

    [Theory]
    [InlineData(UserRole.Candidate)]
    [InlineData(UserRole.Educator)]
    [InlineData(UserRole.Author)]
    public async Task Only_reviewers_and_admins_reach_the_queue(UserRole role)
    {
        var user = await _flags.UserWithRoleAsync(role);

        await (await _flags.QueueAsync(user.AccessToken)).ShouldBeProblemAsync(HttpStatusCode.Forbidden, "FORBIDDEN");
        await (await _flags.UpdateAsync(user.AccessToken, Guid.NewGuid(), new { status = "triaged" }))
            .ShouldBeProblemAsync(HttpStatusCode.Forbidden, "FORBIDDEN");
    }

    [Theory]
    [InlineData(UserRole.Reviewer)]
    [InlineData(UserRole.Admin)]
    public async Task A_reported_question_reaches_the_queue(UserRole role)
    {
        var moderator = await _flags.UserWithRoleAsync(role);
        var flagId = await ReportAsync("rationale_unclear", "The rationale skips step two.");

        var item = await FindInQueueAsync(moderator.AccessToken, flagId, "open");

        item.GetProperty("reason").GetString().ShouldBe("rationale_unclear");
        item.GetProperty("comment").GetString().ShouldBe("The rationale skips step two.");
        item.GetProperty("reporter").GetProperty("displayName").GetString().ShouldBe("Test");
        item.GetProperty("question").GetProperty("version").GetInt32().ShouldBe(1);
        item.GetProperty("question").GetProperty("itemType").GetString().ShouldBe("multiple_choice");
        item.GetProperty("assignee").ValueKind.ShouldBe(JsonValueKind.Null);
    }

    [Fact]
    public async Task The_queue_lists_one_status_oldest_first()
    {
        var moderator = await _flags.UserWithRoleAsync(UserRole.Reviewer);
        var older = await ReportAsync("typo");
        var newer = await ReportAsync("typo");
        await _flags.UpdateOkAsync(moderator.AccessToken, newer, new { status = "triaged" });

        var open = await AllInQueueAsync(moderator.AccessToken, "open");
        var triaged = await AllInQueueAsync(moderator.AccessToken, "triaged");

        open.ShouldContain(older);
        open.ShouldNotContain(newer);
        triaged.ShouldContain(newer);
        var created = await CreatedTimesAsync(moderator.AccessToken, "open");
        created.ShouldBe([.. created.Order()]);
    }

    [Fact]
    public async Task A_moderator_triages_and_takes_a_report()
    {
        var moderator = await _flags.UserWithRoleAsync(UserRole.Reviewer);
        var flagId = await ReportAsync("typo");

        var updated = await _flags.UpdateOkAsync(moderator.AccessToken, flagId, new { status = "triaged", assigneeId = moderator.Body.User.Id });

        updated.GetProperty("status").GetString().ShouldBe("triaged");
        updated.GetProperty("assignee").GetProperty("id").GetGuid().ShouldBe(moderator.Body.User.Id);
    }

    [Fact]
    public async Task Only_a_moderator_can_be_assigned()
    {
        var moderator = await _flags.UserWithRoleAsync(UserRole.Admin);
        var candidate = await _auth.SignupNewUserAsync();
        var flagId = await ReportAsync("typo");

        var problem = await (await _flags.UpdateAsync(moderator.AccessToken, flagId, new { assigneeId = candidate.Body.User.Id }))
            .ShouldBeProblemAsync(HttpStatusCode.BadRequest, "VALIDATION_FAILED");
        problem.FieldErrors().ShouldBe([("assigneeId", "INVALID_VALUE")]);
    }

    [Fact]
    public async Task Resolving_notifies_the_reporter_with_the_note()
    {
        var moderator = await _flags.UserWithRoleAsync(UserRole.Reviewer);
        var reporter = await _scenario.CandidateInAttemptAsync();
        var flagId = await ReportAsync(reporter, "incorrect_answer_key");

        var resolved = await _flags.UpdateOkAsync(moderator.AccessToken, flagId, new
        {
            status = "resolved",
            resolutionNote = "  You were right: option C is also correct, and the key is fixed.  ",
        });

        resolved.GetProperty("status").GetString().ShouldBe("resolved");
        resolved.GetProperty("resolvedBy").GetProperty("id").GetGuid().ShouldBe(moderator.Body.User.Id);
        resolved.GetProperty("resolvedAt").ValueKind.ShouldBe(JsonValueKind.String);
        var notification = (await _notifications.ForAsync(reporter.UserId)).Where(n => n.Type == NotificationType.FlagResolved).ShouldHaveSingleItem();
        notification.Category.ShouldBe(NotificationCategory.Content);
        notification.Title.ShouldBe("Thanks — we reviewed your report");
        notification.Body.ShouldBe("You were right: option C is also correct, and the key is fixed.");
        notification.Channels.ShouldBe([NotificationChannels.InApp]);
        JsonDocument.Parse(notification.Data).RootElement.GetProperty("deepLink").GetString().ShouldBe("/settings/reports");
        var mine = (await FlagsApi.JsonAsync(await _flags.MineAsync(reporter.Token))).GetProperty("items")[0];
        mine.GetProperty("status").GetString().ShouldBe("resolved");
        mine.GetProperty("resolutionNote").GetString().ShouldBe("You were right: option C is also correct, and the key is fixed.");
        (await _audit.ActionsForAsync(moderator.Body.User.Id)).ShouldContain("flag.resolve");
    }

    [Fact]
    public async Task Rejecting_keeps_the_note_from_the_reporter_and_sends_nothing()
    {
        var moderator = await _flags.UserWithRoleAsync(UserRole.Admin);
        var reporter = await _scenario.CandidateInAttemptAsync();
        var flagId = await ReportAsync(reporter, "incorrect_answer_key");

        await _flags.UpdateOkAsync(moderator.AccessToken, flagId, new { status = "rejected", resolutionNote = "Key matches current guidance." });

        (await _notifications.ForAsync(reporter.UserId)).ShouldNotContain(n => n.Type == NotificationType.FlagResolved);
        var mine = (await FlagsApi.JsonAsync(await _flags.MineAsync(reporter.Token))).GetProperty("items")[0];
        mine.GetProperty("status").GetString().ShouldBe("rejected");
        mine.GetProperty("resolutionNote").ValueKind.ShouldBe(JsonValueKind.Null);
        (await _audit.ActionsForAsync(moderator.Body.User.Id)).ShouldContain("flag.reject");
    }

    [Theory]
    [InlineData("resolved")]
    [InlineData("rejected")]
    public async Task A_closed_report_cannot_change(string outcome)
    {
        var moderator = await _flags.UserWithRoleAsync(UserRole.Reviewer);
        var flagId = await ReportAsync("typo");
        await _flags.UpdateOkAsync(moderator.AccessToken, flagId, new { status = outcome, resolutionNote = "Done." });

        await (await _flags.UpdateAsync(moderator.AccessToken, flagId, new { status = "resolved", resolutionNote = "Again." }))
            .ShouldBeProblemAsync(HttpStatusCode.Conflict, "FLAG_CLOSED");
        await (await _flags.UpdateAsync(moderator.AccessToken, flagId, new { assigneeId = moderator.Body.User.Id }))
            .ShouldBeProblemAsync(HttpStatusCode.Conflict, "FLAG_CLOSED");
    }

    [Fact]
    public async Task Resolving_needs_a_note()
    {
        var moderator = await _flags.UserWithRoleAsync(UserRole.Reviewer);
        var flagId = await ReportAsync("typo");

        var problem = await (await _flags.UpdateAsync(moderator.AccessToken, flagId, new { status = "resolved", resolutionNote = "  " }))
            .ShouldBeProblemAsync(HttpStatusCode.BadRequest, "VALIDATION_FAILED");
        problem.FieldErrors().ShouldBe([("resolutionNote", "REQUIRED")]);
    }

    [Theory]
    [InlineData("{\"status\":\"open\"}")]
    [InlineData("{}")]
    public async Task A_report_cannot_be_reopened_or_patched_with_nothing(string body)
    {
        var moderator = await _flags.UserWithRoleAsync(UserRole.Reviewer);
        var flagId = await ReportAsync("typo");

        await (await _flags.UpdateAsync(moderator.AccessToken, flagId, JsonDocument.Parse(body).RootElement))
            .ShouldBeProblemAsync(HttpStatusCode.BadRequest, "VALIDATION_FAILED");
    }

    [Fact]
    public async Task An_unknown_report_is_not_found()
    {
        var moderator = await _flags.UserWithRoleAsync(UserRole.Reviewer);

        await (await _flags.UpdateAsync(moderator.AccessToken, Guid.NewGuid(), new { status = "triaged" }))
            .ShouldBeProblemAsync(HttpStatusCode.NotFound, "NOT_FOUND");
    }

    private async Task<Guid> ReportAsync(string reason, string? comment = null) =>
        await ReportAsync(await _scenario.CandidateInAttemptAsync(), reason, comment);

    private async Task<Guid> ReportAsync(FlagScenario.Candidate reporter, string reason, string? comment = null) =>
        (await _flags.ReportOkAsync(reporter.Token, reporter.VersionIds[0], new { reason, comment, attemptId = reporter.AttemptId }))
            .GetProperty("id").GetGuid();

    // Other tests add reports to the same database, so these walk every page.
    private async Task<List<JsonElement>> QueueItemsAsync(string token, string status)
    {
        var items = new List<JsonElement>();
        string? cursor = null;
        do
        {
            var query = $"?status={status}&limit=50" + (cursor is null ? string.Empty : $"&cursor={cursor}");
            var page = await FlagsApi.JsonAsync(await _flags.QueueAsync(token, query));
            items.AddRange(page.GetProperty("items").EnumerateArray().Select(i => i.Clone()));
            cursor = page.GetProperty("nextCursor").GetString();
        }
        while (cursor is not null);

        return items;
    }

    private async Task<List<Guid>> AllInQueueAsync(string token, string status) =>
        [.. (await QueueItemsAsync(token, status)).Select(i => i.GetProperty("id").GetGuid())];

    private async Task<List<DateTimeOffset>> CreatedTimesAsync(string token, string status) =>
        [.. (await QueueItemsAsync(token, status)).Select(i => i.GetProperty("createdAt").GetDateTimeOffset())];

    private async Task<JsonElement> FindInQueueAsync(string token, Guid flagId, string status) =>
        (await QueueItemsAsync(token, status)).Single(i => i.GetProperty("id").GetGuid() == flagId);
}
