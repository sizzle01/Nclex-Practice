using System.Text.Json;
using Nclex.Domain.Entities;
using Nclex.Domain.Grading;
using Nclex.Domain.ItemTypes;

namespace Nclex.Domain.Tests.Grading;

public sealed class ItemGradingTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 28, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Submitting_a_tutor_item_locks_it_with_its_answer()
    {
        var (attempt, items) = Start(RevealMode.Tutor);
        items[0].SaveResponse(JsonDocument.Parse("""{ "selected": "b" }"""), 1, Now);

        attempt.SubmitItem(items[0], Now.AddSeconds(5));

        items[0].ShouldSatisfyAllConditions(
            i => i.State.ShouldBe(ItemState.Locked),
            i => i.LockedAt.ShouldBe(Now.AddSeconds(5)),
            i => i.Response!.RootElement.GetProperty("selected").GetString().ShouldBe("b"));
        attempt.LastActivityAt.ShouldBe(Now.AddSeconds(5));
    }

    [Fact]
    public void Items_cannot_be_submitted_one_by_one_in_exam_mode()
    {
        var (attempt, items) = Start(RevealMode.Exam);

        Should.Throw<InvalidOperationException>(() => attempt.SubmitItem(items[0], Now));
    }

    [Fact]
    public void Grading_stores_points_and_the_per_target_detail()
    {
        var (attempt, items) = Start(RevealMode.Tutor);
        attempt.SubmitItem(items[0], Now);

        items[0].RecordGrade(
            GradeResult.From(1, 3, [new GradeDetail("a", 1), new GradeDetail("d", -1), new GradeDetail("b", 0, Missed: true)]),
            Now);

        items[0].ShouldSatisfyAllConditions(
            i => i.PointsEarned.ShouldBe(1),
            i => i.MaxPoints.ShouldBe((short)3),
            i => i.IsCorrect.ShouldBe(false),
            i => i.GradedAt.ShouldBe(Now));
        GradeDetailJson.Read(items[0].GradeDetail!).Select(d => (d.Target, d.Delta, d.Missed))
            .ShouldBe([("a", 1, false), ("d", -1, false), ("b", 0, true)]);
        // Empty fields are left out of the stored JSON, as in api.md's reveal example.
        items[0].GradeDetail!.RootElement[0].GetRawText().ShouldBe("""{"target":"a","delta":1,"missed":false}""");
    }

    [Fact]
    public void Only_a_locked_item_can_be_graded_and_only_once()
    {
        var (attempt, items) = Start(RevealMode.Tutor);
        var grade = GradeResult.From(1, 1, []);

        Should.Throw<InvalidOperationException>(() => items[0].RecordGrade(grade, Now));
        attempt.SubmitItem(items[0], Now);
        items[0].RecordGrade(grade, Now);
        Should.Throw<InvalidOperationException>(() => items[0].RecordGrade(grade, Now));
    }

    [Fact]
    public void Tutor_items_are_revealed_once_locked_and_exam_items_only_after_submit()
    {
        var (tutor, tutorItems) = Start(RevealMode.Tutor);
        var (exam, examItems) = Start(RevealMode.Exam);

        RevealRules.CanReveal(tutor, tutorItems[0]).ShouldBeFalse();
        tutor.SubmitItem(tutorItems[0], Now);
        RevealRules.CanReveal(tutor, tutorItems[0]).ShouldBeTrue();
        RevealRules.CanReveal(tutor, tutorItems[1]).ShouldBeFalse();

        exam.Finalize(SubmitReason.User, examItems, Now);
        examItems.ShouldAllBe(i => RevealRules.CanReveal(exam, i));
    }

    [Fact]
    public void A_discarded_attempt_reveals_nothing()
    {
        var (attempt, items) = Start(RevealMode.Tutor);
        attempt.SubmitItem(items[0], Now);

        attempt.Discard(Now);

        RevealRules.CanReveal(attempt, items[0]).ShouldBeFalse();
    }

    private static (Attempt Attempt, IReadOnlyList<AttemptItem> Items) Start(RevealMode revealMode) =>
        Attempt.Start(Guid.NewGuid(), Set(revealMode), [Guid.NewGuid(), Guid.NewGuid()], Now);

    private static TestSet Set(RevealMode revealMode) => TestSet.CreatePrebuilt(
        Guid.NewGuid(),
        ExamTrack.RN,
        new TestSetSettings(
            "Practice 1", null, TestMode.Practice, revealMode,
            Timed: false, PerQuestionSec: null, TimeBudgetSec: null, PassThreshold: null,
            PausePolicy.Unlimited, AllowBackNav: true, IsSample: false, PlanTier.Free),
        questionCount: 2,
        Now);
}
