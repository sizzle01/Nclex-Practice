using System.Globalization;
using Nclex.Domain.Entities;
using Nclex.Domain.Grading;
using Nclex.Domain.Performance;

namespace Nclex.Domain.Tests.Performance;

public sealed class PerformanceSnapshotTests
{
    private static readonly DateTimeOffset Now = DateTimeOffset.Parse("2026-10-28T12:00:00Z", CultureInfo.InvariantCulture);

    [Fact]
    public void Adding_attempts_keeps_running_totals_and_a_weighted_average_time()
    {
        var snapshot = PerformanceSnapshot.Start(Guid.NewGuid(), ExamTrack.RN, PerformanceDimension.ClientNeeds, "SECE.MOC", Now);
        var first = Guid.NewGuid();
        var second = Guid.NewGuid();

        snapshot.Add(first, new SnapshotTally(Items: 2, ItemsCorrect: 1, PointsEarned: 1m, MaxPoints: 2m, TimeMs: 2000), Now);
        snapshot.Add(second, new SnapshotTally(Items: 6, ItemsCorrect: 6, PointsEarned: 7.5m, MaxPoints: 8m, TimeMs: 18000), Now.AddDays(1));

        (snapshot.ItemsAnswered, snapshot.ItemsCorrect, snapshot.PointsEarned, snapshot.MaxPoints)
            .ShouldBe((8, 7, 8.50m, 10.00m));
        snapshot.Accuracy.ShouldBe(0.8500m);
        snapshot.AvgTimeMs.ShouldBe(2500); // (2000 + 18000) / 8
        (snapshot.AttemptsCount, snapshot.LastAttemptId, snapshot.UpdatedAt).ShouldBe((2, second, Now.AddDays(1)));
    }

    [Fact]
    public void A_row_with_nothing_to_earn_has_zero_accuracy()
    {
        var snapshot = PerformanceSnapshot.Start(Guid.NewGuid(), ExamTrack.RN, PerformanceDimension.Overall, PerformanceDimensions.OverallKey, Now);

        snapshot.Add(Guid.NewGuid(), new SnapshotTally(1, 0, 0m, 0m, 0), Now);

        snapshot.Accuracy.ShouldBe(0m);
    }

    [Theory]
    [InlineData(0, 0, 0L, 0, 0)]
    [InlineData(0, 0, 3000L, 2, 1500)]
    [InlineData(1000, 3, 0L, 1, 750)]
    [InlineData(1, 2, 0L, 1, 1)] // 2 / 3 = 0.67 rounds up
    public void Averages_are_combined_by_item_count(int average, int items, long addedTimeMs, int addedItems, int expected) =>
        SnapshotMaths.CombineAverage(average, items, addedTimeMs, addedItems).ShouldBe(expected);

    [Theory]
    [InlineData(PerformanceDimension.Overall, "overall")]
    [InlineData(PerformanceDimension.ClientNeedsParent, "client_needs_parent")]
    [InlineData(PerformanceDimension.CjmmSkill, "cjmm_skill")]
    public void Dimensions_are_named_in_snake_case_and_read_back(PerformanceDimension dimension, string name)
    {
        PerformanceDimensions.Name(dimension).ShouldBe(name);
        PerformanceDimensions.TryParse(name, out var parsed).ShouldBeTrue();
        parsed.ShouldBe(dimension);
    }

    [Fact]
    public void Every_breakdown_dimension_has_a_performance_dimension_of_the_same_name()
    {
        foreach (var dimension in Enum.GetValues<ScoreDimension>())
        {
            var performance = PerformanceDimensions.From(dimension);
            PerformanceDimensions.ToScoreDimension(performance).ShouldBe(dimension);
            PerformanceDimensions.Name(performance).ShouldBe(DimensionKeys.For(dimension));
        }
    }
}
