using Nclex.Domain.Timing;

namespace Nclex.Domain.Tests.Timing;

public sealed class CountdownTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 28, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public void No_timer_has_no_remaining_time_and_never_expires()
    {
        var none = Countdown.None;

        none.ShouldSatisfyAllConditions(
            c => c.HasTimer.ShouldBeFalse(),
            c => c.RemainingAt(Now).ShouldBeNull(),
            c => c.StartAt(Now).ShouldBe(Countdown.None),
            c => c.HasExpiredAt(Now.AddYears(1)).ShouldBeFalse());
    }

    [Fact]
    public void Starting_sets_the_deadline_from_the_remaining_time()
    {
        var running = Countdown.Stopped(90_000).StartAt(Now);

        running.ShouldBe(new Countdown(Now.AddSeconds(90), 90_000));
        running.IsRunning.ShouldBeTrue();
    }

    [Fact]
    public void A_running_countdown_reads_its_remaining_time_from_the_clock()
    {
        var running = Countdown.Stopped(90_000).StartAt(Now);

        running.RemainingAt(Now.AddSeconds(30)).ShouldBe(60_000);
        running.RemainingAt(Now.AddSeconds(90)).ShouldBe(0);
        running.RemainingAt(Now.AddMinutes(10)).ShouldBe(0);
    }

    [Fact]
    public void Starting_a_running_countdown_keeps_its_deadline()
    {
        var running = Countdown.Stopped(90_000).StartAt(Now);

        running.StartAt(Now.AddSeconds(40)).ShouldBe(running);
    }

    [Fact]
    public void Stopping_freezes_the_time_left_and_restarting_continues_from_it()
    {
        var stopped = Countdown.Stopped(90_000).StartAt(Now).StopAt(Now.AddSeconds(30));
        var restarted = stopped.StartAt(Now.AddHours(2));

        stopped.ShouldBe(Countdown.Stopped(60_000));
        stopped.RemainingAt(Now.AddHours(1)).ShouldBe(60_000);
        restarted.DeadlineAt.ShouldBe(Now.AddHours(2).AddSeconds(60));
    }

    [Fact]
    public void Stopping_after_the_deadline_leaves_zero_not_a_negative_time()
    {
        var stopped = Countdown.Stopped(1_000).StartAt(Now).StopAt(Now.AddSeconds(5));

        stopped.RemainingMs.ShouldBe(0);
    }

    [Fact]
    public void Stopped_never_holds_a_negative_time()
    {
        Countdown.Stopped(-500).RemainingMs.ShouldBe(0);
    }

    [Theory]
    [InlineData(0, false)]          // at the deadline
    [InlineData(2_000, false)]      // exactly the end of the grace: still accepted
    [InlineData(2_001, true)]       // 1 ms past the grace
    [InlineData(60_000, true)]
    public void It_expires_only_once_the_write_grace_has_passed(int msAfterDeadline, bool expired)
    {
        var running = Countdown.Stopped(60_000).StartAt(Now);

        running.HasExpiredAt(Now.AddSeconds(60).AddMilliseconds(msAfterDeadline)).ShouldBe(expired);
    }

    [Fact]
    public void A_stopped_countdown_never_expires_even_with_no_time_left()
    {
        Countdown.Stopped(0).HasExpiredAt(Now.AddDays(30)).ShouldBeFalse();
    }
}
