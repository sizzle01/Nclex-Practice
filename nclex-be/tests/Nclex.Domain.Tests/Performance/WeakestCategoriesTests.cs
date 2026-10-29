using Nclex.Domain.Entities;
using Nclex.Domain.Performance;

namespace Nclex.Domain.Tests.Performance;

public sealed class WeakestCategoriesTests
{
    [Fact]
    public void The_three_least_accurate_client_needs_categories_with_enough_items_come_worst_first()
    {
        var snapshots = new[]
        {
            Snapshot(PerformanceDimension.ClientNeeds, "SECE.MOC", items: 10, earned: 8),
            Snapshot(PerformanceDimension.ClientNeeds, "SECE.SIC", items: 10, earned: 4),
            Snapshot(PerformanceDimension.ClientNeeds, "PI.PPT", items: 5, earned: 1),
            Snapshot(PerformanceDimension.ClientNeeds, "HPM", items: 4, earned: 3),
            // Too few items to call weak, and another dimension: both left out however low.
            Snapshot(PerformanceDimension.ClientNeeds, "PSI", items: 2, earned: 0),
            Snapshot(PerformanceDimension.BodySystem, "cardiovascular", items: 10, earned: 0),
        };

        WeakestCategories.From(snapshots).Select(s => s.DimensionKey).ShouldBe(["PI.PPT", "SECE.SIC", "HPM"]);
    }

    [Fact]
    public void On_a_tie_the_category_with_more_items_comes_first()
    {
        var snapshots = new[]
        {
            Snapshot(PerformanceDimension.ClientNeeds, "A", items: 4, earned: 2),
            Snapshot(PerformanceDimension.ClientNeeds, "B", items: 8, earned: 4),
        };

        WeakestCategories.From(snapshots).Select(s => s.DimensionKey).ShouldBe(["B", "A"]);
    }

    private static PerformanceSnapshot Snapshot(PerformanceDimension dimension, string key, int items, decimal earned)
    {
        var snapshot = PerformanceSnapshot.Start(Guid.NewGuid(), ExamTrack.RN, dimension, key, DateTimeOffset.UnixEpoch);
        snapshot.Add(Guid.NewGuid(), new SnapshotTally(items, (int)earned, earned, items, 0), DateTimeOffset.UnixEpoch);
        return snapshot;
    }
}
