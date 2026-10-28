using System.Text.Json;
using Nclex.Domain.Entities;
using Nclex.Domain.Timing;

namespace Nclex.Domain.Tests.Entities;

/// <summary>The timer, pause and expiry rules of architecture.md 5.1 as applied to an attempt.</summary>
public sealed class AttemptTimingTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 28, 9, 0, 0, TimeSpan.Zero);
    private const int BudgetSec = 3_600;

    // Exam timer

    [Fact]
    public void A_timed_attempt_starts_with_its_full_budget_and_the_timer_stopped()
    {
        var (attempt, _) = Start(budgetSec: BudgetSec);

        attempt.ExamTimer.ShouldBe(Countdown.Stopped(BudgetSec * 1000L));
    }

    [Fact]
    public void An_untimed_attempt_has_no_exam_timer()
    {
        var (attempt, items) = Start();
        attempt.Open("c-1", items[0], Now);

        attempt.ExamTimer.ShouldBe(Countdown.None);
        attempt.HasExamExpiredAt(Now.AddYears(1)).ShouldBeFalse();
    }

    [Fact]
    public void The_first_open_starts_the_exam_timer()
    {
        var (attempt, items) = Start(budgetSec: BudgetSec);

        attempt.Open("c-1", items[0], Now.AddMinutes(5));

        attempt.ExamDeadlineAt.ShouldBe(Now.AddMinutes(5).AddSeconds(BudgetSec));
    }

    [Fact]
    public void Opening_again_on_another_device_keeps_the_deadline()
    {
        var (attempt, items) = Start(budgetSec: BudgetSec);
        attempt.Open("laptop", items[0], Now);

        attempt.Open("phone", items[0], Now.AddMinutes(20));

        attempt.ExamDeadlineAt.ShouldBe(Now.AddSeconds(BudgetSec));
        attempt.ExamTimer.RemainingAt(Now.AddMinutes(20)).ShouldBe((BudgetSec - 1_200) * 1000L);
    }

    [Fact]
    public void The_exam_expires_only_after_the_deadline_plus_the_write_grace()
    {
        var (attempt, items) = Start(budgetSec: BudgetSec);
        attempt.Open("c-1", items[0], Now);
        var deadline = Now.AddSeconds(BudgetSec);

        attempt.HasExamExpiredAt(deadline.AddSeconds(2)).ShouldBeFalse();
        attempt.HasExamExpiredAt(deadline.AddSeconds(2).AddMilliseconds(1)).ShouldBeTrue();
    }

    // Pause

    [Fact]
    public void Pausing_freezes_the_exam_timer_and_moves_the_epoch_on()
    {
        var (attempt, items) = Opened(budgetSec: BudgetSec);
        var epoch = attempt.TimerEpoch;

        var outcome = attempt.Pause(PauseReason.Break, items[0], Now.AddMinutes(10));

        outcome.ShouldBe(PauseOutcome.Paused);
        attempt.ShouldSatisfyAllConditions(
            a => a.Status.ShouldBe(AttemptStatus.Paused),
            a => a.ExamTimer.ShouldBe(Countdown.Stopped((BudgetSec - 600) * 1000L)),
            a => a.PauseReason.ShouldBe(PauseReason.Break),
            a => a.PauseStartedAt.ShouldBe(Now.AddMinutes(10)),
            a => a.PausesUsed.ShouldBe((short)1),
            a => a.TimerEpoch.ShouldBe(epoch + 1));
        items[0].TimeSpentMs.ShouldBe(600_000);
        items[0].EnteredAt.ShouldBeNull();
    }

    [Fact]
    public void A_paused_attempt_never_expires_however_long_it_waits()
    {
        var (attempt, items) = Opened(budgetSec: BudgetSec);
        attempt.Pause(PauseReason.SaveExit, items[0], Now.AddMinutes(1));

        attempt.HasExamExpiredAt(Now.AddDays(90)).ShouldBeFalse();
        attempt.ExamTimer.RemainingAt(Now.AddDays(90)).ShouldBe((BudgetSec - 60) * 1000L);
    }

    [Fact]
    public void Breaks_and_save_and_exit_both_count_against_the_pause_limit()
    {
        var (attempt, items) = Opened(budgetSec: BudgetSec, pausePolicy: new PausePolicy(MaxPauses: 2, MaxPauseSec: 600));
        attempt.Pause(PauseReason.Break, items[0], Now.AddMinutes(1));
        attempt.Resume("c-1", items[0], Now.AddMinutes(2));
        attempt.Pause(PauseReason.SaveExit, items[0], Now.AddMinutes(3));
        attempt.Resume("c-1", items[0], Now.AddMinutes(4));

        var third = attempt.Pause(PauseReason.Break, items[0], Now.AddMinutes(5));

        third.ShouldBe(PauseOutcome.LimitReached);
        attempt.Status.ShouldBe(AttemptStatus.InProgress);
        attempt.PausesUsed.ShouldBe((short)2);
    }

    [Fact]
    public void Practice_mode_pauses_without_limit()
    {
        var (attempt, items) = Opened(budgetSec: BudgetSec);

        for (var i = 0; i < 20; i++)
        {
            attempt.Pause(PauseReason.Break, items[0], Now.AddMinutes(i)).ShouldBe(PauseOutcome.Paused);
            attempt.Resume("c-1", items[0], Now.AddMinutes(i).AddSeconds(30));
        }

        attempt.PausesUsed.ShouldBe((short)20);
    }

    [Fact]
    public void Only_an_attempt_in_progress_can_be_paused()
    {
        var (attempt, items) = Opened(budgetSec: BudgetSec);
        attempt.Pause(PauseReason.Break, items[0], Now);

        Should.Throw<InvalidOperationException>(() => attempt.Pause(PauseReason.Break, items[0], Now));
    }

    // Resume

    [Fact]
    public void Resuming_within_the_limit_restarts_the_timer_with_the_frozen_time()
    {
        var (attempt, items) = Opened(budgetSec: BudgetSec, pausePolicy: new PausePolicy(3, 600));
        attempt.Pause(PauseReason.Break, items[0], Now.AddMinutes(10));

        var outcome = attempt.Resume("phone", items[0], Now.AddMinutes(18));

        outcome.ShouldBe(ResumeOutcome.Resumed);
        attempt.ShouldSatisfyAllConditions(
            a => a.Status.ShouldBe(AttemptStatus.InProgress),
            a => a.ExamDeadlineAt.ShouldBe(Now.AddMinutes(18).AddSeconds(BudgetSec - 600)),
            a => a.PauseStartedAt.ShouldBeNull(),
            a => a.PauseReason.ShouldBeNull(),
            a => a.TotalPausedMs.ShouldBe(480_000),
            a => a.ActiveClientId.ShouldBe("phone"));
        items[0].EnteredAt.ShouldBe(Now.AddMinutes(18));
    }

    [Fact]
    public void A_break_past_its_limit_uses_up_exam_time()
    {
        var (attempt, items) = Opened(budgetSec: BudgetSec, pausePolicy: new PausePolicy(3, 600));
        attempt.Pause(PauseReason.Break, items[0], Now.AddMinutes(10)); // 50 min left

        attempt.Resume("c-1", items[0], Now.AddMinutes(25)); // 15 min break, limit 10

        attempt.ExamTimer.RemainingAt(Now.AddMinutes(25)).ShouldBe(45 * 60_000L);
    }

    [Fact]
    public void Save_and_exit_past_the_limit_uses_up_exam_time_too()
    {
        var (attempt, items) = Opened(budgetSec: BudgetSec, pausePolicy: new PausePolicy(3, 600));
        attempt.Pause(PauseReason.SaveExit, items[0], Now.AddMinutes(10)); // 50 min left

        attempt.Resume("c-1", items[0], Now.AddMinutes(40)); // 30 min away, limit 10

        attempt.ExamTimer.RemainingAt(Now.AddMinutes(40)).ShouldBe(30 * 60_000L);
    }

    [Fact]
    public void An_overrun_that_uses_up_all_the_time_reports_time_expired_and_leaves_the_timer_stopped()
    {
        var (attempt, items) = Opened(budgetSec: BudgetSec, pausePolicy: new PausePolicy(3, 600));
        attempt.Pause(PauseReason.SaveExit, items[0], Now.AddMinutes(10)); // 50 min left

        var outcome = attempt.Resume("c-1", items[0], Now.AddDays(2));

        outcome.ShouldBe(ResumeOutcome.TimeExpired);
        attempt.ExamTimer.ShouldBe(Countdown.Stopped(0));
        items[0].EnteredAt.ShouldBeNull();
    }

    [Fact]
    public void An_overrun_that_leaves_exactly_nothing_is_time_expired()
    {
        var (attempt, items) = Opened(budgetSec: BudgetSec, pausePolicy: new PausePolicy(3, 600));
        attempt.Pause(PauseReason.Break, items[0], Now.AddMinutes(10)); // 50 min left

        attempt.Resume("c-1", items[0], Now.AddMinutes(70)).ShouldBe(ResumeOutcome.TimeExpired); // 60 min break, limit 10
    }

    [Fact]
    public void Each_pause_and_resume_moves_the_epoch_on_so_older_jobs_are_stale()
    {
        var (attempt, items) = Opened(budgetSec: BudgetSec);
        var atOpen = attempt.TimerEpoch;

        attempt.Pause(PauseReason.Break, items[0], Now.AddMinutes(1));
        var atPause = attempt.TimerEpoch;
        attempt.Resume("c-1", items[0], Now.AddMinutes(2));

        (atOpen, atPause, attempt.TimerEpoch).ShouldBe((0, 1, 2));
    }

    [Fact]
    public void Only_a_paused_attempt_can_be_resumed()
    {
        var (attempt, items) = Opened(budgetSec: BudgetSec);

        Should.Throw<InvalidOperationException>(() => attempt.Resume("c-1", items[0], Now));
    }

    // Item timers

    [Fact]
    public void Without_a_per_question_limit_items_have_no_timer()
    {
        var (_, items) = Opened(budgetSec: BudgetSec);

        items[0].ItemTimer.ShouldBe(Countdown.None);
    }

    [Fact]
    public void Entering_an_item_starts_its_timer_with_the_per_question_limit()
    {
        var (_, items) = Opened(perQuestionSec: 90);

        items[0].ItemTimer.ShouldBe(new Countdown(Now.AddSeconds(90), 90_000));
    }

    [Fact]
    public void Leaving_an_item_freezes_its_timer_and_coming_back_continues_it()
    {
        var (attempt, items) = Opened(perQuestionSec: 90);

        attempt.Navigate(items[0], items[1], Now.AddSeconds(30));
        var frozen = items[0].ItemTimer;
        attempt.Navigate(items[1], items[0], Now.AddSeconds(35));

        frozen.ShouldBe(Countdown.Stopped(60_000));
        items[0].ItemTimer.DeadlineAt.ShouldBe(Now.AddSeconds(35).AddSeconds(60));
        items[1].ItemTimer.ShouldBe(Countdown.Stopped(85_000));
    }

    [Fact]
    public void An_item_whose_time_ran_out_is_locked_with_its_answer()
    {
        var (attempt, items) = Opened(perQuestionSec: 90);
        items[0].SaveResponse(Json("""{"selected":"b"}"""), 1, Now.AddSeconds(10));

        var beforeGrace = attempt.ExpireItemTimer(items[0], Now.AddSeconds(92));
        var afterGrace = attempt.ExpireItemTimer(items[0], Now.AddSeconds(95));

        (beforeGrace, afterGrace).ShouldBe((false, true));
        items[0].ShouldSatisfyAllConditions(
            i => i.State.ShouldBe(ItemState.Locked),
            i => i.Response!.RootElement.GetProperty("selected").GetString().ShouldBe("b"),
            i => i.ItemTimer.ShouldBe(Countdown.Stopped(0)),
            i => i.TimeSpentMs.ShouldBe(90_000)); // counted up to the deadline, not to when it was noticed
        items[0].SaveResponse(Json("""{"selected":"c"}"""), 2, Now.AddSeconds(96)).ShouldBe(SaveOutcome.ItemLocked);
    }

    [Fact]
    public void Coming_back_to_an_item_with_no_time_left_locks_it()
    {
        var (attempt, items) = Opened(perQuestionSec: 90);
        attempt.Navigate(items[0], items[1], Now.AddSeconds(91)); // left inside the grace, with 0 left

        attempt.Navigate(items[1], items[0], Now.AddSeconds(100));

        items[0].IsLocked.ShouldBeTrue();
    }

    [Fact]
    public void Pausing_freezes_the_item_timer_and_resuming_restarts_it()
    {
        var (attempt, items) = Opened(perQuestionSec: 90);

        attempt.Pause(PauseReason.Break, items[0], Now.AddSeconds(20));
        var frozen = items[0].ItemTimer;
        attempt.Resume("c-1", items[0], Now.AddHours(1));

        frozen.ShouldBe(Countdown.Stopped(70_000));
        items[0].ItemTimer.DeadlineAt.ShouldBe(Now.AddHours(1).AddSeconds(70));
    }

    [Fact]
    public void Reopening_after_the_tab_was_closed_does_not_give_item_time_back()
    {
        var (attempt, items) = Opened(perQuestionSec: 90);

        attempt.Open("c-2", items[0], Now.AddSeconds(60));

        items[0].ItemTimer.DeadlineAt.ShouldBe(Now.AddSeconds(90));
    }

    [Fact]
    public void Item_timers_do_not_expire_while_the_attempt_is_paused()
    {
        var (attempt, items) = Opened(perQuestionSec: 90);
        attempt.Pause(PauseReason.Break, items[0], Now.AddSeconds(20));

        attempt.ExpireItemTimer(items[0], Now.AddHours(1)).ShouldBeFalse();
    }

    // Finalize

    [Fact]
    public void Finalizing_submits_the_attempt_and_locks_every_item_with_its_answer()
    {
        var (attempt, items) = Opened(budgetSec: BudgetSec);
        items[0].SaveResponse(Json("""{"selected":"b"}"""), 1, Now);
        var at = Now.AddSeconds(BudgetSec + 5);

        attempt.Finalize(SubmitReason.TimeExpired, items, at);

        attempt.ShouldSatisfyAllConditions(
            a => a.Status.ShouldBe(AttemptStatus.Submitted),
            a => a.SubmitReason.ShouldBe(SubmitReason.TimeExpired),
            a => a.SubmittedAt.ShouldBe(at),
            a => a.ExamTimer.ShouldBe(Countdown.Stopped(0)),
            a => a.ActiveClientId.ShouldBeNull());
        items.ShouldAllBe(i => i.State == ItemState.Locked);
        items[0].Response.ShouldNotBeNull();
        items[1].Response.ShouldBeNull(); // graded blank in F10
        attempt.CheckWrite("c-1").ShouldBe(AttemptWriteCheck.TimeExpired);
    }

    [Fact]
    public void Finalizing_needs_every_item_and_a_draft()
    {
        var (attempt, items) = Opened(budgetSec: BudgetSec);

        Should.Throw<ArgumentException>(() => attempt.Finalize(SubmitReason.TimeExpired, [items[0]], Now));
        attempt.Finalize(SubmitReason.TimeExpired, items, Now);
        Should.Throw<InvalidOperationException>(() => attempt.Finalize(SubmitReason.TimeExpired, items, Now));
    }

    private static (Attempt Attempt, IReadOnlyList<AttemptItem> Items) Opened(
        int? budgetSec = null,
        int? perQuestionSec = null,
        PausePolicy? pausePolicy = null)
    {
        var (attempt, items) = Start(budgetSec, perQuestionSec, pausePolicy);
        attempt.Open("c-1", items[0], Now);
        return (attempt, items);
    }

    private static (Attempt Attempt, IReadOnlyList<AttemptItem> Items) Start(
        int? budgetSec = null,
        int? perQuestionSec = null,
        PausePolicy? pausePolicy = null)
    {
        var timed = budgetSec is not null || perQuestionSec is not null;
        var set = TestSet.CreatePrebuilt(
            Guid.NewGuid(),
            ExamTrack.RN,
            new TestSetSettings(
                "Challenge 1", null, TestMode.Challenge, RevealMode.Exam,
                timed, perQuestionSec, budgetSec, PassThreshold: null,
                pausePolicy ?? PausePolicy.Unlimited, AllowBackNav: true, IsSample: false, PlanTier.Free),
            questionCount: 3,
            Now);
        return Attempt.Start(Guid.NewGuid(), set, [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()], Now);
    }

    private static JsonDocument Json(string json) => JsonDocument.Parse(json);
}
