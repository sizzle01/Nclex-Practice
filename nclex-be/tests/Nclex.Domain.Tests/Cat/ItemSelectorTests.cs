using Nclex.Domain.Builder;
using Nclex.Domain.Cat;

namespace Nclex.Domain.Tests.Cat;

public sealed class ItemSelectorTests
{
    private const short Care = 2;
    private const short Safety = 3;

    // Management of Care weighs 18, Safety and Infection Prevention and Control 13 (test-plan midpoints).
    private static readonly LeafWeight[] Plan = [new(Care, 18m), new(Safety, 13m)];

    [Fact]
    public void An_empty_pool_gives_no_question()
    {
        ItemSelector.Choose([], 0, Counts(), Plan, new Random(1)).ShouldBeNull();
    }

    [Fact]
    public void The_leaf_furthest_behind_its_share_goes_next()
    {
        var pool = Pool(Care, 10).Concat(Pool(Safety, 10)).ToList();

        // After 3 Care and none of Safety, Safety's share of 4 questions (1.68) is furthest behind.
        var pick = ItemSelector.Choose(pool, 0, Counts((Care, 3)), Plan, new Random(1));

        pick!.ClientNeedsId.ShouldBe(Safety);
    }

    [Fact]
    public void With_nothing_given_yet_the_heaviest_leaf_goes_first()
    {
        var pool = Pool(Care, 5).Concat(Pool(Safety, 5)).ToList();

        ItemSelector.Choose(pool, 0, Counts(), Plan, new Random(1))!.ClientNeedsId.ShouldBe(Care);
    }

    [Fact]
    public void A_leaf_with_no_questions_left_is_skipped()
    {
        var pool = Pool(Care, 5);

        ItemSelector.Choose(pool, 0, Counts((Care, 9)), Plan, new Random(1))!.ClientNeedsId.ShouldBe(Care);
    }

    [Theory]
    [InlineData(2.0, 1.0)]
    [InlineData(0.0, 0.0)]
    [InlineData(-2.0, -1.0)]
    public void The_pick_is_among_the_questions_closest_to_the_ability(double theta, double expectedDifficulty)
    {
        // Six questions at each seeded difficulty: the top 5 by information are all at the nearest one.
        var pool = new[] { -1.0, 0.0, 1.0 }
            .SelectMany(b => Enumerable.Range(0, 6).Select(_ => Candidate(Care, b)))
            .ToList();

        for (var seed = 0; seed < 20; seed++)
        {
            ItemSelector.Choose(pool, theta, Counts(), Plan, new Random(seed))!.Difficulty.ShouldBe(expectedDifficulty);
        }
    }

    [Fact]
    public void Different_seeds_spread_the_picks_and_the_same_seed_repeats_one()
    {
        var pool = Pool(Care, 20);

        var picks = Enumerable.Range(0, 30).Select(seed => ItemSelector.Choose(pool, 0, Counts(), Plan, new Random(seed))!.QuestionId).ToHashSet();
        var first = ItemSelector.Choose(pool, 0, Counts(), Plan, new Random(7));
        var again = ItemSelector.Choose(pool, 0, Counts(), Plan, new Random(7));

        picks.Count.ShouldBeGreaterThan(5); // randomesque and the shuffle reach past the first 5
        again.ShouldBe(first);
    }

    [Fact]
    public void Without_test_plan_weights_the_whole_pool_is_used()
    {
        var pool = Pool(Care, 3);

        ItemSelector.Choose(pool, 0, Counts(), [], new Random(1)).ShouldNotBeNull();
    }

    private static List<CatCandidate> Pool(short leaf, int count) =>
        [.. Enumerable.Range(0, count).Select(_ => Candidate(leaf, 0))];

    private static CatCandidate Candidate(short leaf, double difficulty) =>
        new(Guid.NewGuid(), Guid.NewGuid(), difficulty, leaf);

    private static Dictionary<short, int> Counts(params (short Leaf, int Count)[] counts) =>
        counts.ToDictionary(c => c.Leaf, c => c.Count);
}
