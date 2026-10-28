using Microsoft.Extensions.DependencyInjection;
using Nclex.Api.IntegrationTests.Infrastructure;
using Nclex.Application.Notifications.DraftReminders;
using Nclex.Domain.Notifications;

namespace Nclex.Api.IntegrationTests.Notifications;

/// <summary>
/// The reminder job's use case (architecture.md 5.3): one reminder per stretch of inactivity, for
/// drafts only. Drafts are aged by moving <c>last_activity_at</c> back, with the real clock.
/// </summary>
public sealed class DraftReminderTests(ApiFactory factory)
{
    private readonly ExamScenario _exam = new(factory, new AttemptsApi(factory, new AuthApi(factory)));
    private readonly NotificationsApi _notifications = new(new AuthApi(factory));
    private readonly NotificationProbe _probe = new(factory);

    [Fact]
    public async Task An_inactive_draft_is_reminded_once()
    {
        var exam = await _exam.StartAsync(new { revealMode = "exam" });
        await AgeDraftAsync(exam.AttemptId, days: 4);

        await RunJobAsync();
        await RunJobAsync();

        var item = (await _notifications.ListOkAsync(exam.Token)).GetProperty("items").EnumerateArray().ShouldHaveSingleItem();
        item.GetProperty("type").GetString().ShouldBe("draft_reminder");
        item.GetProperty("category").GetString().ShouldBe("reminders");
        item.GetProperty("title").GetString().ShouldBe("Pick up where you left off");
        item.GetProperty("body").GetString().ShouldBe($"{exam.SetTitle} is 0% done.");
        item.GetProperty("data").GetProperty("deepLink").GetString().ShouldBe($"/attempts/{exam.AttemptId}");
    }

    [Fact]
    public async Task Activity_after_a_reminder_earns_another_one_later()
    {
        var exam = await _exam.StartAsync(new { revealMode = "exam" });
        await AgeDraftAsync(exam.AttemptId, days: 20);
        await RunJobAsync();
        await _probe.ExecuteAsync(
            $"UPDATE notifications SET created_at = now() - interval '15 days' WHERE related_attempt_id = {exam.AttemptId}");
        await AgeDraftAsync(exam.AttemptId, days: 4);

        await RunJobAsync();

        (await _probe.ForAsync(exam.UserId)).Count(n => n.Type == NotificationType.DraftReminder).ShouldBe(2);
    }

    [Fact]
    public async Task Recent_drafts_and_submitted_attempts_are_left_alone()
    {
        var recent = await _exam.StartAsync(new { revealMode = "exam" });
        var submitted = await _exam.StartAsync(new { revealMode = "exam" });
        await _exam.SubmitOkAsync(submitted);
        await AgeDraftAsync(submitted.AttemptId, days: 4);

        await RunJobAsync();

        (await _probe.ForAsync(recent.UserId)).ShouldBeEmpty();
        (await _probe.ForAsync(submitted.UserId)).ShouldNotContain(n => n.Type == NotificationType.DraftReminder);
    }

    [Fact]
    public async Task With_reminders_switched_off_nothing_is_sent()
    {
        var exam = await _exam.StartAsync(new { revealMode = "exam" });
        await _notifications.ReplacePreferencesOkAsync(exam.Token, new
        {
            preferences = new[] { "results", "reminders", "engagement", "content", "security" }
                .Select(category => new { category, inApp = category != "reminders", email = category is "security" or "results" }),
        });
        await AgeDraftAsync(exam.AttemptId, days: 4);

        await RunJobAsync();

        (await _probe.ForAsync(exam.UserId)).ShouldBeEmpty();
    }

    private Task AgeDraftAsync(Guid attemptId, int days) =>
        _probe.ExecuteAsync($"UPDATE attempts SET last_activity_at = now() - make_interval(days => {days}) WHERE id = {attemptId}");

    private async Task RunJobAsync()
    {
        // The API host does not register the worker's use case, so it is built here with the defaults.
        await using var scope = factory.Services.CreateAsyncScope();
        var handler = ActivatorUtilities.CreateInstance<SendDraftRemindersHandler>(scope.ServiceProvider, DraftReminderSettings.Default);
        await handler.HandleAsync(TestContext.Current.CancellationToken);
    }
}
