using System.Text.Json;
using Nclex.Domain.Entities;

namespace Nclex.Domain.Tests.Entities;

public sealed class TestSetTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 27, 12, 0, 0, TimeSpan.Zero);

    private static readonly TestSetSettings Untimed = new(
        "Comprehensive Practice 1", null, TestMode.Practice, RevealMode.Tutor,
        Timed: false, PerQuestionSec: null, TimeBudgetSec: null, PassThreshold: null,
        PausePolicy.Unlimited, AllowBackNav: true, IsSample: false, PlanTier.Free);

    [Fact]
    public void A_prebuilt_set_is_published_with_its_settings()
    {
        var id = Guid.NewGuid();

        var set = TestSet.CreatePrebuilt(id, ExamTrack.RN, Untimed, questionCount: 40, Now);

        set.ShouldSatisfyAllConditions(
            s => s.Id.ShouldBe(id),
            s => s.Kind.ShouldBe(TestSetKind.Prebuilt),
            s => s.IsPublished.ShouldBeTrue(),
            s => s.OwnerUserId.ShouldBeNull(),
            s => s.QuestionCount.ShouldBe(40),
            s => s.Settings.ShouldBe(Untimed));
    }

    [Fact]
    public void A_custom_set_belongs_to_its_owner_and_is_not_published()
    {
        var owner = Guid.NewGuid();
        using var criteria = JsonDocument.Parse("""{"pool":"unused"}""");

        var set = TestSet.CreateCustom(owner, ExamTrack.PN, Untimed, questionCount: 30, criteria, Now);

        set.ShouldSatisfyAllConditions(
            s => s.Id.ShouldNotBe(Guid.Empty),
            s => s.Kind.ShouldBe(TestSetKind.Custom),
            s => s.OwnerUserId.ShouldBe(owner),
            s => s.IsPublished.ShouldBeFalse(),
            s => s.Track.ShouldBe(ExamTrack.PN),
            s => s.QuestionCount.ShouldBe(30),
            s => s.GeneratorCriteria.ShouldBeSameAs(criteria));
    }

    [Fact]
    public void Applying_new_settings_replaces_them()
    {
        var set = TestSet.CreatePrebuilt(Guid.NewGuid(), ExamTrack.RN, Untimed, 40, Now);
        var challenge = Untimed with
        {
            Mode = TestMode.Challenge,
            RevealMode = RevealMode.Exam,
            Timed = true,
            TimeBudgetSec = 4800,
            PassThreshold = 0.65m,
            PausePolicy = new PausePolicy(2, 300),
        };

        set.Apply(challenge, 30);

        set.Settings.ShouldBe(challenge);
        set.QuestionCount.ShouldBe(30);
    }

    [Fact]
    public void A_timed_set_without_a_timer_is_rejected() =>
        Should.Throw<ArgumentException>(() =>
            TestSet.CreatePrebuilt(Guid.NewGuid(), ExamTrack.RN, Untimed with { Timed = true }, 40, Now));

    [Fact]
    public void An_untimed_set_with_a_timer_is_rejected() =>
        Should.Throw<ArgumentException>(() =>
            TestSet.CreatePrebuilt(Guid.NewGuid(), ExamTrack.RN, Untimed with { PerQuestionSec = 90 }, 40, Now));

    [Fact]
    public void A_set_needs_at_least_one_question() =>
        Should.Throw<ArgumentOutOfRangeException>(() => TestSet.CreatePrebuilt(Guid.NewGuid(), ExamTrack.RN, Untimed, 0, Now));

    [Theory]
    [InlineData(false, null, null, true)]
    [InlineData(true, 90, null, true)]
    [InlineData(true, null, 4800, true)]
    [InlineData(true, 90, 4800, true)]
    [InlineData(true, null, null, false)]
    [InlineData(false, 90, null, false)]
    [InlineData(false, null, 4800, false)]
    public void Timing_is_consistent_when_timed_sets_have_a_timer_and_untimed_ones_have_none(
        bool timed, int? perQuestionSec, int? timeBudgetSec, bool consistent) =>
        (Untimed with { Timed = timed, PerQuestionSec = perQuestionSec, TimeBudgetSec = timeBudgetSec })
            .HasConsistentTiming.ShouldBe(consistent);

    [Fact]
    public void Items_are_numbered_from_one_in_the_order_given()
    {
        var setId = Guid.NewGuid();
        Guid[] questions = [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()];

        var items = TestSetItem.ForQuestions(setId, questions);

        items.Select(i => (i.TestSetId, i.Position, i.QuestionId)).ShouldBe(
            [(setId, 1, questions[0]), (setId, 2, questions[1]), (setId, 3, questions[2])]);
    }
}
