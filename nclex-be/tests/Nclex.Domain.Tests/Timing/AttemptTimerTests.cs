using Nclex.Domain.Timing;

namespace Nclex.Domain.Tests.Timing;

public sealed class AttemptTimerTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 28, 9, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData(0, null, true)]     // practice: unlimited
    [InlineData(50, null, true)]
    [InlineData(0, 2, true)]
    [InlineData(1, 2, true)]
    [InlineData(2, 2, false)]
    [InlineData(0, 0, false)]       // a set that allows no pauses
    public void Pauses_are_allowed_until_the_limit_is_used(int used, int? max, bool allowed)
    {
        AttemptTimer.CanPause(used, max).ShouldBe(allowed);
    }

    [Theory]
    [InlineData(300, 600, 0)]       // within the limit: no cost
    [InlineData(600, 600, 0)]       // exactly the limit
    [InlineData(601, 600, 1_000)]   // 1 s over
    [InlineData(900, 600, 300_000)] // 5 min over
    public void A_pause_costs_only_the_time_past_its_limit(int pausedSec, int maxPauseSec, long overrunMs)
    {
        AttemptTimer.OverrunMs(Now, Now.AddSeconds(pausedSec), maxPauseSec).ShouldBe(overrunMs);
    }

    [Fact]
    public void Without_a_pause_limit_a_pause_costs_nothing()
    {
        AttemptTimer.OverrunMs(Now, Now.AddDays(3), maxPauseSec: null).ShouldBe(0);
    }

    [Fact]
    public void After_a_pause_the_overrun_comes_off_the_remaining_time()
    {
        // architecture.md 5.1: remaining = remaining_at_pause - max(0, paused - maxPauseSec)
        var paused = Countdown.Stopped(1_800_000); // 30 min left

        var after = AttemptTimer.AfterPause(paused, Now, Now.AddMinutes(15), maxPauseSec: 600);

        after.ShouldBe(Countdown.Stopped(1_500_000)); // 5 min over the 10 min limit
        after.IsRunning.ShouldBeFalse();
    }

    [Fact]
    public void An_overrun_longer_than_the_time_left_leaves_zero()
    {
        var after = AttemptTimer.AfterPause(Countdown.Stopped(60_000), Now, Now.AddHours(1), maxPauseSec: 600);

        after.RemainingMs.ShouldBe(0);
    }

    [Fact]
    public void A_pause_of_an_untimed_exam_leaves_it_untimed()
    {
        AttemptTimer.AfterPause(Countdown.None, Now, Now.AddHours(1), maxPauseSec: 60).ShouldBe(Countdown.None);
    }

    [Fact]
    public void The_expiry_job_runs_just_after_the_write_grace()
    {
        var deadline = Now.AddMinutes(80);
        var jobAt = AttemptTimer.ExpiryJobAt(deadline);

        Countdown.Stopped(80 * 60_000).StartAt(Now).HasExpiredAt(jobAt).ShouldBeTrue();
        jobAt.ShouldBe(deadline.AddSeconds(3));
    }

    [Fact]
    public void The_sweeper_cutoff_is_30_seconds_ago()
    {
        AttemptTimer.SweepCutoff(Now).ShouldBe(Now.AddSeconds(-30));
    }
}
