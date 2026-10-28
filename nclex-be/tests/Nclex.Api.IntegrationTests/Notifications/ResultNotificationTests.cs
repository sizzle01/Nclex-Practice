using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Nclex.Api.IntegrationTests.Infrastructure;
using Nclex.Application.Common;
using Nclex.Application.Email;
using Nclex.Domain.Notifications;
using Nclex.Infrastructure.Persistence;

namespace Nclex.Api.IntegrationTests.Notifications;

/// <summary>
/// The result notification written in the submit transaction (architecture.md 5.5): exactly one
/// per attempt, on the channels the user's preferences allow.
/// </summary>
public sealed class ResultNotificationTests(ApiFactory factory)
{
    private static readonly string[] ResultTemplates =
        [EmailTemplateNames.ResultPass, EmailTemplateNames.ResultFail, EmailTemplateNames.ResultReady];

    private readonly ExamScenario _exam = new(factory, new AttemptsApi(factory, new AuthApi(factory)));
    private readonly NotificationsApi _notifications = new(new AuthApi(factory));
    private readonly NotificationProbe _probe = new(factory);
    private readonly OutboxProbe _outbox = new(factory);

    [Fact]
    public async Task Falling_short_of_the_threshold_notifies_result_fail_in_app_and_by_email()
    {
        var exam = await _exam.StartAsync(new { revealMode = "exam", passThreshold = 0.5 });

        await _exam.SubmitOkAsync(exam);

        var item = (await _notifications.ListOkAsync(exam.Token)).GetProperty("items").EnumerateArray().ShouldHaveSingleItem();
        item.GetProperty("type").GetString().ShouldBe("result_fail");
        item.GetProperty("category").GetString().ShouldBe("results");
        item.GetProperty("title").GetString().ShouldBe("You didn't reach the passing threshold this time");
        item.GetProperty("body").GetString()!.ShouldStartWith("You scored 0%. Here's what to focus on: ");
        item.GetProperty("relatedAttemptId").GetGuid().ShouldBe(exam.AttemptId);
        item.GetProperty("readAt").ValueKind.ShouldBe(System.Text.Json.JsonValueKind.Null);
        var data = item.GetProperty("data");
        data.GetProperty("deepLink").GetString().ShouldBe($"/attempts/{exam.AttemptId}/results#breakdown");
        data.GetProperty("scorePct").GetDecimal().ShouldBe(0m);
        data.GetProperty("weakest").GetArrayLength().ShouldBeGreaterThan(0);
        (await _outbox.ForAsync(exam.Email)).Where(m => ResultTemplates.Contains(m.Template))
            .ShouldHaveSingleItem().Template.ShouldBe(EmailTemplateNames.ResultFail);
    }

    [Fact]
    public async Task Reaching_the_threshold_notifies_result_pass()
    {
        var exam = await _exam.StartAsync(new { revealMode = "exam", passThreshold = 0.5 });
        await _exam.AnswerAllCorrectlyAsync(exam);

        await _exam.SubmitOkAsync(exam);

        var item = (await _notifications.ListOkAsync(exam.Token)).GetProperty("items").EnumerateArray().ShouldHaveSingleItem();
        item.GetProperty("type").GetString().ShouldBe("result_pass");
        item.GetProperty("body").GetString().ShouldBe($"You scored 100% on {exam.SetTitle}. Review your rationales to lock it in.");
    }

    [Fact]
    public async Task A_set_without_a_threshold_notifies_result_ready()
    {
        var exam = await _exam.StartAsync(new { revealMode = "exam" });

        await _exam.SubmitOkAsync(exam);

        (await _probe.ForAsync(exam.UserId)).ShouldHaveSingleItem().Type.ShouldBe(NotificationType.ResultReady);
    }

    [Fact]
    public async Task Submitting_again_does_not_notify_twice()
    {
        var exam = await _exam.StartAsync(new { revealMode = "exam", passThreshold = 0.5 });

        await _exam.SubmitOkAsync(exam);
        await _exam.SubmitOkAsync(exam);

        (await _probe.ForAsync(exam.UserId)).Count(n => Notification.ResultTypes.Contains(n.Type)).ShouldBe(1);
        (await _outbox.ForAsync(exam.Email)).Count(m => ResultTemplates.Contains(m.Template)).ShouldBe(1);
        (await _notifications.UnreadCountAsync(exam.Token)).ShouldBe(1);
    }

    [Fact]
    public async Task The_database_refuses_a_second_result_notification_for_the_same_attempt()
    {
        var exam = await _exam.StartAsync(new { revealMode = "exam", passThreshold = 0.5 });
        await _exam.SubmitOkAsync(exam);

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Notifications.Add(Notification.Create(
            exam.UserId,
            NotificationType.ResultPass,
            NotificationCategory.Results,
            "Duplicate",
            "Duplicate",
            "{}",
            exam.AttemptId,
            [NotificationChannels.InApp],
            DateTimeOffset.UtcNow));

        var save = () => db.SaveChangesAsync(TestContext.Current.CancellationToken);

        (await save.ShouldThrowAsync<UniqueConstraintViolationException>()).ConstraintName.ShouldBe("notif_result_once");
    }

    [Fact]
    public async Task With_result_emails_off_only_the_bell_hears_about_it()
    {
        var exam = await _exam.StartAsync(new { revealMode = "exam", passThreshold = 0.5 });
        await _notifications.ReplacePreferencesOkAsync(exam.Token, NotificationsApi.Preferences());

        await _exam.SubmitOkAsync(exam);

        (await _notifications.UnreadCountAsync(exam.Token)).ShouldBe(1);
        (await _outbox.ForAsync(exam.Email)).ShouldNotContain(m => ResultTemplates.Contains(m.Template));
    }

    [Fact]
    public async Task With_results_in_app_off_the_email_still_goes_but_the_feed_stays_empty()
    {
        var exam = await _exam.StartAsync(new { revealMode = "exam", passThreshold = 0.5 });
        await _notifications.ReplacePreferencesOkAsync(exam.Token, NotificationsApi.Preferences(inAppResults: false, "results"));

        await _exam.SubmitOkAsync(exam);

        (await _notifications.ListOkAsync(exam.Token)).GetProperty("items").GetArrayLength().ShouldBe(0);
        (await _notifications.UnreadCountAsync(exam.Token)).ShouldBe(0);
        (await _probe.ForAsync(exam.UserId)).ShouldHaveSingleItem().Channels.ShouldBe([NotificationChannels.Email]);
        (await _outbox.ForAsync(exam.Email)).ShouldContain(m => m.Template == EmailTemplateNames.ResultFail);
    }

    [Fact]
    public async Task With_live_updates_down_no_stream_ticket_is_issued()
    {
        // The shared host's Redis is unreachable by design; the client then polls the unread count.
        var exam = await _exam.StartAsync(new { revealMode = "exam" });

        await (await _notifications.StreamTicketAsync(exam.Token)).ShouldBeProblemAsync(HttpStatusCode.ServiceUnavailable, "STREAM_UNAVAILABLE");
    }
}
