using Nclex.Domain.Builder;

namespace Nclex.Domain.Tests.Builder;

public sealed class WeakAreaSamplerTests
{
    private const int Seed = 20261117;

    private static readonly LeafWeight[] OneLeaf = [new(7, 15m)];
    private static readonly LeafWeight[] TwoLeaves = [new(2, 18m), new(3, 13m)]; // RN SECE.MOC and SECE.SIC

    private static List<WeakAreaCandidate> Questions(short leaf, PastResult result, int count) =>
        [.. Enumerable.Range(0, count).Select(_ => new WeakAreaCandidate(Guid.NewGuid(), leaf, result))];

    private static int CountOf(IEnumerable<Guid> ids, IEnumerable<WeakAreaCandidate> group) =>
        ids.Count(id => group.Any(c => c.QuestionId == id));

    [Fact]
    public void Missed_questions_are_picked_before_unseen_and_unseen_before_correct()
    {
        var missed = Questions(7, PastResult.Missed, 4);
        var unseen = Questions(7, PastResult.Unseen, 5);
        var correct = Questions(7, PastResult.Correct, 30);

        var ids = WeakAreaSampler.Sample([.. correct, .. unseen, .. missed], 12, OneLeaf, new Random(Seed));

        ids.Count.ShouldBe(12);
        CountOf(ids, missed).ShouldBe(4);
        CountOf(ids, unseen).ShouldBe(5);
        CountOf(ids, correct).ShouldBe(3);
    }

    [Fact]
    public void Correct_questions_are_left_out_when_the_others_are_enough()
    {
        var unseen = Questions(7, PastResult.Unseen, 40);
        var correct = Questions(7, PastResult.Correct, 40);

        var ids = WeakAreaSampler.Sample([.. unseen, .. correct], 25, OneLeaf, new Random(Seed));

        CountOf(ids, correct).ShouldBe(0);
    }

    [Fact]
    public void Several_weak_areas_share_the_test_by_test_plan_weight()
    {
        var moc = Questions(2, PastResult.Unseen, 50);
        var sic = Questions(3, PastResult.Unseen, 50);

        var ids = WeakAreaSampler.Sample([.. moc, .. sic], 31, TwoLeaves, new Random(Seed));

        // 31 split 18:13 is exactly 18 and 13.
        CountOf(ids, moc).ShouldBe(18);
        CountOf(ids, sic).ShouldBe(13);
    }

    [Fact]
    public void A_short_area_leaves_its_places_to_the_other_areas_missed_first()
    {
        var moc = Questions(2, PastResult.Missed, 3);
        var sicMissed = Questions(3, PastResult.Missed, 15);
        var sicCorrect = Questions(3, PastResult.Correct, 40);

        var ids = WeakAreaSampler.Sample([.. moc, .. sicMissed, .. sicCorrect], 25, TwoLeaves, new Random(Seed));

        ids.Count.ShouldBe(25);
        CountOf(ids, moc).ShouldBe(3);
        CountOf(ids, sicMissed).ShouldBe(15);
        CountOf(ids, sicCorrect).ShouldBe(7);
    }

    [Fact]
    public void A_small_pool_gives_a_smaller_test_without_repeats()
    {
        var pool = Questions(2, PastResult.Missed, 6).Concat(Questions(3, PastResult.Unseen, 5)).ToList();

        var ids = WeakAreaSampler.Sample(pool, 25, TwoLeaves, new Random(Seed));

        ids.Count.ShouldBe(11);
        ids.ShouldBeUnique();
    }

    [Fact]
    public void The_same_seed_gives_the_same_test_whatever_the_pool_order()
    {
        var pool = Questions(2, PastResult.Missed, 20).Concat(Questions(3, PastResult.Unseen, 20)).ToList();
        var reversed = Enumerable.Reverse(pool).ToList();

        WeakAreaSampler.Sample(pool, 10, TwoLeaves, new Random(Seed))
            .ShouldBe(WeakAreaSampler.Sample(reversed, 10, TwoLeaves, new Random(Seed)));
    }
}
