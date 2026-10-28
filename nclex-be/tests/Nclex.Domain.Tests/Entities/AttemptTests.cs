using System.Text.Json;
using Nclex.Domain.Entities;

namespace Nclex.Domain.Tests.Entities;

public sealed class AttemptTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 28, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public void A_new_attempt_starts_at_item_1_with_the_set_settings_and_no_lease()
    {
        var set = Set(allowBackNav: false);
        var versions = new[] { Guid.NewGuid(), Guid.NewGuid() };

        var (attempt, items) = Attempt.Start(Guid.NewGuid(), set, versions, Now);

        attempt.ShouldSatisfyAllConditions(
            a => a.Status.ShouldBe(AttemptStatus.InProgress),
            a => a.TestSetId.ShouldBe(set.Id),
            a => a.TotalItems.ShouldBe(2),
            a => a.CurrentPosition.ShouldBe(1),
            a => a.ActiveClientId.ShouldBeNull(),
            a => a.Settings.AllowBackNav.ShouldBeFalse(),
            a => a.Settings.Mode.ShouldBe(TestMode.Practice));
        items.Select(i => (i.Position, i.QuestionVersionId, i.State))
            .ShouldBe([(1, versions[0], ItemState.Unseen), (2, versions[1], ItemState.Unseen)]);
    }

    [Fact]
    public void Starting_without_items_is_refused()
    {
        Should.Throw<ArgumentException>(() => Attempt.Start(Guid.NewGuid(), Set(), [], Now));
    }

    [Fact]
    public void Only_the_client_holding_the_lease_may_write()
    {
        var (attempt, items) = Start();

        attempt.CheckWrite("c-1").ShouldBe(AttemptWriteCheck.OpenedElsewhere);
        attempt.Open("c-1", items[0], Now);
        attempt.CheckWrite("c-1").ShouldBe(AttemptWriteCheck.Allowed);

        attempt.Open("c-2", items[0], Now.AddMinutes(1));
        attempt.CheckWrite("c-1").ShouldBe(AttemptWriteCheck.OpenedElsewhere);
        attempt.CheckWrite("c-2").ShouldBe(AttemptWriteCheck.Allowed);
    }

    [Fact]
    public void A_discarded_attempt_cannot_be_written_or_opened()
    {
        var (attempt, items) = Start();
        attempt.Open("c-1", items[0], Now);

        attempt.Discard(Now.AddMinutes(1));

        attempt.Status.ShouldBe(AttemptStatus.Abandoned);
        attempt.CheckWrite("c-1").ShouldBe(AttemptWriteCheck.NotActive);
        Should.Throw<InvalidOperationException>(() => attempt.Open("c-1", items[0], Now));
    }

    [Fact]
    public void Discarding_twice_changes_nothing_the_second_time()
    {
        var (attempt, _) = Start();
        attempt.Discard(Now.AddMinutes(1));

        attempt.Discard(Now.AddMinutes(5));

        attempt.LastActivityAt.ShouldBe(Now.AddMinutes(1));
    }

    [Fact]
    public void Opening_marks_the_current_item_seen_and_starts_its_clock()
    {
        var (attempt, items) = Start();

        attempt.Open("c-1", items[0], Now);

        items[0].State.ShouldBe(ItemState.Seen);
        items[0].EnteredAt.ShouldBe(Now);
    }

    [Fact]
    public void Navigating_records_time_on_the_item_left_and_starts_the_next_one()
    {
        var (attempt, items) = Start();
        attempt.Open("c-1", items[0], Now);

        var outcome = attempt.Navigate(items[0], items[1], Now.AddSeconds(45));

        outcome.ShouldBe(NavigationOutcome.Moved);
        attempt.CurrentPosition.ShouldBe(2);
        items[0].TimeSpentMs.ShouldBe(45_000);
        items[0].EnteredAt.ShouldBeNull();
        (items[1].State, items[1].EnteredAt).ShouldBe((ItemState.Seen, Now.AddSeconds(45)));
    }

    [Fact]
    public void Time_on_an_item_adds_up_over_visits()
    {
        var (attempt, items) = Start();
        attempt.Open("c-1", items[0], Now);
        attempt.Navigate(items[0], items[1], Now.AddSeconds(10));
        attempt.Navigate(items[1], items[0], Now.AddSeconds(20));

        attempt.Navigate(items[0], items[1], Now.AddSeconds(25));

        items[0].TimeSpentMs.ShouldBe(15_000);
        items[1].TimeSpentMs.ShouldBe(10_000);
    }

    [Fact]
    public void Taking_over_counts_the_old_device_time_only_up_to_its_last_activity()
    {
        var (attempt, items) = Start();
        attempt.Open("c-1", items[0], Now);
        attempt.RecordActivity(Now.AddSeconds(30));

        // The first tab closed; the candidate comes back two hours later on another device.
        attempt.Open("c-2", items[0], Now.AddHours(2));

        items[0].TimeSpentMs.ShouldBe(30_000);
        items[0].EnteredAt.ShouldBe(Now.AddHours(2));
    }

    [Fact]
    public void Without_back_navigation_going_back_is_refused()
    {
        var (attempt, items) = Start(allowBackNav: false);
        attempt.Open("c-1", items[0], Now);
        attempt.Navigate(items[0], items[1], Now);

        var outcome = attempt.Navigate(items[1], items[0], Now);

        outcome.ShouldBe(NavigationOutcome.BackNavigationDisabled);
        attempt.CurrentPosition.ShouldBe(2);
    }

    [Fact]
    public void Without_back_navigation_moving_forward_locks_the_item_left()
    {
        var (attempt, items) = Start(allowBackNav: false);
        attempt.Open("c-1", items[0], Now);

        attempt.Navigate(items[0], items[1], Now.AddSeconds(5));

        (items[0].State, items[0].LockedAt).ShouldBe((ItemState.Locked, Now.AddSeconds(5)));
        items[1].IsLocked.ShouldBeFalse();
    }

    [Fact]
    public void With_back_navigation_items_stay_open_after_leaving_them()
    {
        var (attempt, items) = Start();
        attempt.Open("c-1", items[0], Now);

        attempt.Navigate(items[0], items[1], Now);

        items[0].IsLocked.ShouldBeFalse();
    }

    [Fact]
    public void Navigating_needs_the_current_item()
    {
        var (attempt, items) = Start();
        attempt.Open("c-1", items[0], Now);

        Should.Throw<InvalidOperationException>(() => attempt.Navigate(items[1], items[0], Now));
    }

    [Theory]
    [InlineData(30, false)]
    [InlineData(31, true)]
    public void A_draft_is_stale_after_30_days_without_activity(int days, bool stale)
    {
        var (attempt, _) = Start();

        attempt.IsStaleDraftAt(Now.AddDays(days)).ShouldBe(stale);
    }

    [Fact]
    public void A_discarded_attempt_is_never_a_stale_draft()
    {
        var (attempt, _) = Start();
        attempt.Discard(Now);

        attempt.IsStaleDraftAt(Now.AddDays(90)).ShouldBeFalse();
    }

    [Fact]
    public void Saving_an_answer_marks_the_item_answered_and_clearing_it_marks_it_seen()
    {
        var (_, items) = Start();

        items[0].SaveResponse(Json("""{"selected":"b"}"""), 1, Now).ShouldBe(SaveOutcome.Saved);
        (items[0].State, items[0].ClientSeq, items[0].AnsweredAt).ShouldBe((ItemState.Answered, 1, Now));

        items[0].SaveResponse(null, 2, Now).ShouldBe(SaveOutcome.Saved);
        (items[0].State, items[0].Response, items[0].AnsweredAt).ShouldBe((ItemState.Seen, null, null));
    }

    [Theory]
    [InlineData(5)]
    [InlineData(4)]
    public void A_write_that_is_not_newer_than_the_stored_one_is_ignored(int staleSeq)
    {
        var (_, items) = Start();
        items[0].SaveResponse(Json("""{"selected":"b"}"""), 5, Now);

        var outcome = items[0].SaveResponse(Json("""{"selected":"c"}"""), staleSeq, Now);

        outcome.ShouldBe(SaveOutcome.Stale);
        items[0].Response!.RootElement.GetProperty("selected").GetString().ShouldBe("b");
        items[0].ClientSeq.ShouldBe(5);
    }

    [Fact]
    public void A_locked_item_keeps_its_answer_and_marks()
    {
        var (attempt, items) = Start(allowBackNav: false);
        attempt.Open("c-1", items[0], Now);
        items[0].SaveResponse(Json("""{"selected":"b"}"""), 1, Now);
        attempt.Navigate(items[0], items[1], Now);

        items[0].SaveResponse(Json("""{"selected":"c"}"""), 2, Now).ShouldBe(SaveOutcome.ItemLocked);
        items[0].UpdateMarks(flagged: true, struckOptions: null).ShouldBeFalse();

        items[0].ClientSeq.ShouldBe(1);
        items[0].Flagged.ShouldBeFalse();
        items[0].IsAnswered.ShouldBeTrue();
    }

    [Fact]
    public void Marks_change_only_what_is_given()
    {
        var (_, items) = Start();
        items[0].UpdateMarks(flagged: true, struckOptions: ["a", "a", "c"]);

        items[0].UpdateMarks(flagged: null, struckOptions: null);

        items[0].Flagged.ShouldBeTrue();
        items[0].StruckOptions.ShouldBe(["a", "c"]);
    }

    private static (Attempt Attempt, IReadOnlyList<AttemptItem> Items) Start(bool allowBackNav = true) =>
        Attempt.Start(Guid.NewGuid(), Set(allowBackNav), [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()], Now);

    private static TestSet Set(bool allowBackNav = true) => TestSet.CreatePrebuilt(
        Guid.NewGuid(),
        ExamTrack.RN,
        new TestSetSettings(
            "Practice 1", null, TestMode.Practice, RevealMode.Tutor,
            Timed: false, PerQuestionSec: null, TimeBudgetSec: null, PassThreshold: null,
            PausePolicy.Unlimited, allowBackNav, IsSample: false, PlanTier.Free),
        questionCount: 3,
        Now);

    private static JsonDocument Json(string json) => JsonDocument.Parse(json);
}
