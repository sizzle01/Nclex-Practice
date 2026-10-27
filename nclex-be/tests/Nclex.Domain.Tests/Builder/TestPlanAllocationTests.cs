using Nclex.Domain.Builder;

namespace Nclex.Domain.Tests.Builder;

public sealed class TestPlanAllocationTests
{
    // RN Physiological Integrity leaves: midpoints of 6-12, 13-19, 9-15 and 11-17%.
    private static readonly LeafWeight[] Phi = [new(7, 9m), new(8, 16m), new(9, 12m), new(10, 14m)];

    private static readonly Dictionary<short, int> Plenty = Phi.ToDictionary(l => l.ClientNeedsId, _ => 100);

    [Fact]
    public void Shares_follow_the_weights_and_add_up_to_the_count()
    {
        var quotas = TestPlanAllocation.Allocate(20, Phi, Plenty);

        // Exact shares 3.53, 6.27, 4.71 and 5.49: floors 3, 6, 4 and 5, then the two largest
        // remainders (leaves 9 and 7) get one more each.
        quotas.ShouldBe(new Dictionary<short, int> { [7] = 4, [8] = 6, [9] = 5, [10] = 5 }, ignoreOrder: true);
        quotas.Values.Sum().ShouldBe(20);
    }

    [Fact]
    public void A_share_is_capped_at_what_the_leaf_has()
    {
        var available = new Dictionary<short, int>(Plenty) { [7] = 2 };

        var quotas = TestPlanAllocation.Allocate(20, Phi, available);

        quotas[7].ShouldBe(2);
        quotas.Values.Sum().ShouldBe(18); // the builder fills the other 2 at random
    }

    [Fact]
    public void A_leaf_with_no_questions_gets_none()
    {
        var available = new Dictionary<short, int>(Plenty);
        available.Remove(8);

        TestPlanAllocation.Allocate(20, Phi, available)[8].ShouldBe(0);
    }

    [Fact]
    public void Equal_weights_split_evenly_with_ties_going_to_the_lower_id()
    {
        LeafWeight[] two = [new(5, 10m), new(3, 10m)];

        var quotas = TestPlanAllocation.Allocate(5, two, new Dictionary<short, int> { [3] = 10, [5] = 10 });

        (quotas[3], quotas[5]).ShouldBe((3, 2));
    }

    [Fact]
    public void No_leaves_means_no_quotas() =>
        TestPlanAllocation.Allocate(10, [], Plenty).ShouldBeEmpty();
}
