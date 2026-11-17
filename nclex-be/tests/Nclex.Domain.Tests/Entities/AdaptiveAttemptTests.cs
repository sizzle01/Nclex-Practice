using Nclex.Domain.Cat;
using Nclex.Domain.Entities;

namespace Nclex.Domain.Tests.Entities;

public sealed class AdaptiveAttemptTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 17, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public void An_adaptive_set_has_no_items_and_counts_its_maximum()
    {
        var set = AdaptiveSet();

        set.ShouldSatisfyAllConditions(
            s => s.Kind.ShouldBe(TestSetKind.Cat),
            s => s.Mode.ShouldBe(TestMode.Cat),
            s => s.QuestionCount.ShouldBe(150),
            s => s.AdaptiveConfig().ShouldBe(CatConfig.NclexRn));
    }

    [Fact]
    public void A_timed_start_keeps_the_set_budget_and_holds_one_item()
    {
        var (attempt, first) = Start(timed: true);

        attempt.ShouldSatisfyAllConditions(
            a => a.IsAdaptive.ShouldBeTrue(),
            a => a.TotalItems.ShouldBe(1),
            a => a.Settings.Timed.ShouldBeTrue(),
            a => a.ExamRemainingMs.ShouldBe(18_000_000),
            a => a.ReadCatState().Items.Count.ShouldBe(1));
        first.Position.ShouldBe(1);
    }

    [Fact]
    public void An_untimed_start_drops_every_timer()
    {
        var (attempt, _) = Start(timed: false);

        attempt.ShouldSatisfyAllConditions(
            a => a.Settings.Timed.ShouldBeFalse(),
            a => a.Settings.TimeBudgetSec.ShouldBeNull(),
            a => a.ExamRemainingMs.ShouldBeNull(),
            a => a.ExamTimer.HasTimer.ShouldBeFalse());
    }

    [Fact]
    public void Answering_locks_the_item_and_the_next_one_becomes_current()
    {
        var (attempt, first) = Start(timed: true);
        attempt.Open("c-1", first, Now);

        attempt.LockAdaptiveAnswer(first, Now.AddSeconds(30));
        var state = attempt.ReadCatState().RecordAnswer(1).Give(new CatStateItem(Guid.NewGuid(), 0, 2));
        var next = attempt.GiveNextAdaptiveItem(first, Guid.NewGuid(), state, Now.AddSeconds(30));

        first.IsLocked.ShouldBeTrue();
        first.TimeSpentMs.ShouldBe(30_000);
        next.Position.ShouldBe(2);
        attempt.TotalItems.ShouldBe(2);
        attempt.CurrentPosition.ShouldBe(2);
        attempt.ReadCatState().AnsweredCount.ShouldBe(1);
    }

    [Fact]
    public void The_next_item_waits_for_the_current_answer()
    {
        var (attempt, first) = Start(timed: true);
        attempt.Open("c-1", first, Now);

        Should.Throw<InvalidOperationException>(() =>
            attempt.GiveNextAdaptiveItem(first, Guid.NewGuid(), attempt.ReadCatState(), Now));
    }

    [Fact]
    public void A_set_that_is_not_adaptive_cannot_start_an_adaptive_attempt()
    {
        var practice = TestSet.CreatePrebuilt(
            Guid.NewGuid(),
            ExamTrack.RN,
            Settings(TestMode.Practice),
            questionCount: 3,
            Now);

        Should.Throw<ArgumentException>(() =>
            Attempt.StartAdaptive(Guid.NewGuid(), practice, timed: false, Guid.NewGuid(), FirstState(), Now));
    }

    [Fact]
    public void An_adaptive_attempt_is_finalized_with_the_items_it_was_given()
    {
        var (attempt, first) = Start(timed: false);
        attempt.Open("c-1", first, Now);

        attempt.Finalize(SubmitReason.CatStop, [first], Now.AddMinutes(1));

        attempt.Status.ShouldBe(AttemptStatus.Submitted);
        attempt.SubmitReason.ShouldBe(SubmitReason.CatStop);
    }

    private static (Attempt Attempt, AttemptItem First) Start(bool timed) =>
        Attempt.StartAdaptive(Guid.NewGuid(), AdaptiveSet(), timed, Guid.NewGuid(), FirstState(), Now);

    private static CatState FirstState() => CatState.Begin(CatConfig.NclexRn, new CatStateItem(Guid.NewGuid(), 0, 2));

    private static TestSet AdaptiveSet() =>
        TestSet.CreateAdaptive(Guid.NewGuid(), ExamTrack.RN, Settings(TestMode.Cat, timeBudgetSec: 18_000), CatConfig.NclexRn, Now);

    private static TestSetSettings Settings(TestMode mode, int? timeBudgetSec = null) => new(
        "NCLEX-RN adaptive exam", null, mode, RevealMode.Exam,
        Timed: timeBudgetSec is not null, PerQuestionSec: null, TimeBudgetSec: timeBudgetSec, PassThreshold: null,
        PausePolicy.Unlimited, AllowBackNav: false, IsSample: false, PlanTier.Free);
}
