using Nclex.Domain.Notifications;

namespace Nclex.Domain.Tests.Notifications;

public sealed class DraftReminderPolicyTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 28, 9, 0, 0, TimeSpan.Zero);
    private static readonly TimeSpan ThreeDays = DraftReminderPolicy.DefaultInactivity;

    [Fact]
    public void A_draft_inactive_for_less_than_the_threshold_is_not_reminded()
    {
        DraftReminderPolicy.IsDue(Now.AddDays(-3).AddMinutes(1), null, Now, ThreeDays).ShouldBeFalse();
    }

    [Fact]
    public void A_draft_inactive_for_the_threshold_is_reminded()
    {
        DraftReminderPolicy.IsDue(Now.AddDays(-3), null, Now, ThreeDays).ShouldBeTrue();
    }

    [Fact]
    public void A_draft_is_reminded_once_per_stretch_of_inactivity()
    {
        var lastActivity = Now.AddDays(-10);

        DraftReminderPolicy.IsDue(lastActivity, lastReminderAt: Now.AddDays(-7), Now, ThreeDays).ShouldBeFalse();
    }

    [Fact]
    public void Activity_after_a_reminder_starts_a_new_stretch()
    {
        var reminded = Now.AddDays(-10);
        var lastActivity = Now.AddDays(-4);

        DraftReminderPolicy.IsDue(lastActivity, reminded, Now, ThreeDays).ShouldBeTrue();
    }
}
