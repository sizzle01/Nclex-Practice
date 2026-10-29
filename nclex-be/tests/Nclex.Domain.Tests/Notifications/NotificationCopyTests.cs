using Nclex.Domain.Notifications;

namespace Nclex.Domain.Tests.Notifications;

public sealed class NotificationCopyTests
{
    [Theory]
    [InlineData("0.58", "58")]
    [InlineData("0.585", "59")]     // halves round up, as a candidate would expect
    [InlineData("0.5849", "58")]
    [InlineData("1", "100")]
    [InlineData("0", "0")]
    public void Shares_become_whole_percentages(string share, string percent)
    {
        NotificationCopy.Percent(decimal.Parse(share, System.Globalization.CultureInfo.InvariantCulture)).ShouldBe(percent);
    }

    [Fact]
    public void A_pass_names_the_set_and_the_score()
    {
        var text = NotificationCopy.ResultPass(0.82m, "Mock exam 1");

        text.Title.ShouldBe("You passed your mock exam — great work!");
        text.Body.ShouldBe("You scored 82% on Mock exam 1. Review your rationales to lock it in.");
    }

    [Fact]
    public void A_fail_names_the_two_weakest_categories()
    {
        var text = NotificationCopy.ResultFail(0.58m, ["Pharmacological Therapies", "Reduction of Risk Potential", "Basic Care"]);

        text.Title.ShouldBe("You didn't reach the passing threshold this time");
        text.Body.ShouldBe("You scored 58%. Here's what to focus on: Pharmacological Therapies, Reduction of Risk Potential.");
    }

    [Fact]
    public void A_fail_with_no_breakdown_still_reads_well()
    {
        NotificationCopy.ResultFail(0.4m, []).Body.ShouldBe("You scored 40%. Review your rationales to see what to focus on.");
    }

    [Fact]
    public void A_set_without_a_threshold_just_reports_the_score()
    {
        var text = NotificationCopy.ResultReady(0.7m, "Cardio quiz");

        (text.Title, text.Body).ShouldBe(("Your results are ready", "You scored 70% on Cardio quiz."));
    }

    [Fact]
    public void A_draft_reminder_says_how_far_along_and_how_much_time_is_left()
    {
        NotificationCopy.DraftReminder("Mock exam 2", 0.4m, 5_400_000).Body.ShouldBe("Mock exam 2 is 40% done with 1 h 30 min left.");
    }

    [Fact]
    public void An_untimed_draft_reminder_leaves_out_the_time()
    {
        NotificationCopy.DraftReminder("Practice set", 0.25m, timeLeftMs: null).Body.ShouldBe("Practice set is 25% done.");
    }

    [Theory]
    [InlineData(59_999, "less than a minute")]
    [InlineData(60_000, "1 min")]
    [InlineData(3_540_000, "59 min")]
    [InlineData(3_600_000, "1 h")]
    [InlineData(7_500_000, "2 h 5 min")]
    public void Durations_are_written_in_hours_and_minutes(long milliseconds, string text)
    {
        NotificationCopy.Duration(milliseconds).ShouldBe(text);
    }

    [Fact]
    public void A_lock_notice_says_until_when_in_utc()
    {
        var text = NotificationCopy.AccountLocked(new DateTimeOffset(2026, 10, 28, 15, 30, 0, TimeSpan.FromHours(1)));

        text.Body.ShouldContain("until 28 Oct 2026 at 14:30 UTC");
    }

    [Fact]
    public void A_resolved_report_thanks_the_reporter_and_shows_the_note_as_written()
    {
        var text = NotificationCopy.FlagResolved("You were right; the key is fixed.");

        text.Title.ShouldBe("Thanks — we reviewed your report");
        text.Body.ShouldBe("You were right; the key is fixed.");
    }
}
