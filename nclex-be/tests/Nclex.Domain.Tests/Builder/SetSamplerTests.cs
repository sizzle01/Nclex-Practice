using Nclex.Domain.Builder;

namespace Nclex.Domain.Tests.Builder;

public sealed class SetSamplerTests
{
    private const int Seed = 20261027;

    private static readonly LeafWeight[] TwoLeaves = [new(2, 18m), new(3, 13m)]; // RN SECE.MOC and SECE.SIC

    private static List<PoolItem> Singles(short leaf, int count) =>
        [.. Enumerable.Range(0, count).Select(_ => new PoolItem(Guid.NewGuid(), leaf))];

    private static List<PoolItem> CaseStudy(short leaf, int items)
    {
        var caseId = Guid.NewGuid();
        // Listed out of order on purpose: the sampler must restore case order.
        return [.. Enumerable.Range(1, items).Reverse().Select(p => new PoolItem(Guid.NewGuid(), leaf, caseId, (short)p))];
    }

    [Fact]
    public void Several_leaves_are_sampled_in_test_plan_proportion()
    {
        var moc = Singles(2, 50);
        var sic = Singles(3, 50);

        var ids = SetSampler.Sample([.. moc, .. sic], 31, TwoLeaves, new Random(Seed));

        // 31 split 18:13 is exactly 18 and 13.
        ids.Count(id => moc.Any(i => i.QuestionId == id)).ShouldBe(18);
        ids.Count(id => sic.Any(i => i.QuestionId == id)).ShouldBe(13);
    }

    [Fact]
    public void A_short_leaf_is_topped_up_at_random_from_the_rest()
    {
        var moc = Singles(2, 3);
        var sic = Singles(3, 50);

        var ids = SetSampler.Sample([.. moc, .. sic], 20, TwoLeaves, new Random(Seed));

        ids.Count.ShouldBe(20);
        ids.Count(id => moc.Any(i => i.QuestionId == id)).ShouldBe(3);
    }

    [Fact]
    public void A_set_never_repeats_a_question()
    {
        var pool = Singles(2, 40).Concat(Singles(3, 40)).Concat(CaseStudy(2, 6)).ToList();

        var ids = SetSampler.Sample(pool, 60, TwoLeaves, new Random(Seed));

        ids.Count.ShouldBe(60);
        ids.ShouldBeUnique();
        ids.ShouldAllBe(id => pool.Any(i => i.QuestionId == id));
    }

    [Fact]
    public void The_same_seed_gives_the_same_set_whatever_the_pool_order()
    {
        var pool = Singles(2, 30).Concat(Singles(3, 30)).ToList();
        var shuffled = pool.ToArray();
        new Random(1).Shuffle(shuffled);

        var first = SetSampler.Sample(pool, 25, TwoLeaves, new Random(Seed));
        var second = SetSampler.Sample(shuffled, 25, TwoLeaves, new Random(Seed));

        second.ShouldBe(first);
    }

    [Fact]
    public void A_case_study_stays_whole_and_in_case_order()
    {
        var caseStudy = CaseStudy(2, 6);
        var pool = caseStudy.Concat(Singles(2, 4)).ToList();

        var ids = SetSampler.Sample(pool, 10, [], new Random(Seed)).ToList();

        var expectedOrder = caseStudy.OrderBy(i => i.CasePosition).Select(i => i.QuestionId).ToList();
        var start = ids.IndexOf(expectedOrder[0]);
        ids.Skip(start).Take(6).ShouldBe(expectedOrder);
    }

    [Fact]
    public void A_case_study_that_does_not_fit_is_left_out()
    {
        var caseStudy = CaseStudy(2, 6);
        var singles = Singles(2, 5);

        var ids = SetSampler.Sample([.. caseStudy, .. singles], 5, [], new Random(Seed));

        ids.ShouldBe(singles.Select(i => i.QuestionId), ignoreOrder: true);
    }

    [Fact]
    public void Too_small_a_pool_gives_everything_it_has()
    {
        var pool = Singles(2, 3).Concat(CaseStudy(3, 6)).ToList();

        var ids = SetSampler.Sample(pool, 20, [], new Random(Seed));

        // The caller sees 9 of 20 and reports INSUFFICIENT_QUESTIONS.
        ids.ShouldBe(pool.Select(i => i.QuestionId), ignoreOrder: true);
    }

    [Fact]
    public void With_one_leaf_or_none_the_sample_is_simply_random()
    {
        var pool = Singles(2, 10);

        var ids = SetSampler.Sample(pool, 10, [new LeafWeight(2, 18m)], new Random(Seed));

        ids.ShouldBe(pool.Select(i => i.QuestionId), ignoreOrder: true);
    }
}
