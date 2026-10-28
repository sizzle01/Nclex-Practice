using Nclex.Domain.Entities;

namespace Nclex.Domain.Performance;

/// <summary>What one attempt adds to one snapshot row: its items in that category and their score.</summary>
/// <param name="Items">Items served, answered or not, as on the results page.</param>
public sealed record SnapshotTally(int Items, int ItemsCorrect, decimal PointsEarned, decimal MaxPoints, long TimeMs)
{
    /// <summary>The attempt's totals, for the <see cref="PerformanceDimension.Overall"/> row.</summary>
    public static SnapshotTally From(AttemptResult result) =>
        new(result.ItemsTotal, result.ItemsCorrect, result.PointsEarned, result.MaxPoints, result.TotalTimeMs);

    /// <summary>One row of the attempt's breakdown.</summary>
    public static SnapshotTally From(AttemptCategoryScore score) =>
        new(score.Items, score.ItemsCorrect, score.PointsEarned, score.MaxPoints, score.TimeMs);
}
