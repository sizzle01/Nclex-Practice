using Nclex.Domain.Grading;
using Nclex.Domain.Performance;

namespace Nclex.Domain.Entities;

/// <summary>
/// A user's running totals in one category of one track, across every submitted attempt, e.g.
/// "Management of Care: 41 items, 68%". Maps to <c>performance_snapshots</c> (database.md section
/// 2.7). The worker keeps it up to date after each submit, so the dashboard and the performance
/// page read one row per category instead of adding up every attempt.
/// </summary>
public sealed class PerformanceSnapshot
{
    // For EF Core, which sets properties directly when loading rows.
    private PerformanceSnapshot()
    {
    }

    public Guid UserId { get; private set; }

    public ExamTrack Track { get; private set; }

    public PerformanceDimension Dimension { get; private set; }

    /// <summary>A Client Needs code, body-system slug, item type..., or "all" for the overall row.</summary>
    public string DimensionKey { get; private set; } = string.Empty;

    /// <summary>Items served in this category, answered or not (the same count as a result's breakdown).</summary>
    public int ItemsAnswered { get; private set; }

    public int ItemsCorrect { get; private set; }

    public decimal PointsEarned { get; private set; }

    public decimal MaxPoints { get; private set; }

    /// <summary>
    /// Points earned as a share of the maximum, 0 to 1. Postgres computes the stored value (a
    /// generated column); <see cref="Add"/> works it out the same way so a row just changed reads right.
    /// </summary>
    public decimal Accuracy { get; private set; }

    public int AvgTimeMs { get; private set; }

    /// <summary>How many attempts had at least one item in this category.</summary>
    public int AttemptsCount { get; private set; }

    /// <summary>
    /// The latest attempt added to this row. On the overall row, which every attempt touches, it is
    /// the high-water mark that makes the refresh job idempotent (see RefreshSnapshotsHandler).
    /// </summary>
    public Guid? LastAttemptId { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    /// <summary>An empty row, before any attempt is added.</summary>
    public static PerformanceSnapshot Start(
        Guid userId,
        ExamTrack track,
        PerformanceDimension dimension,
        string dimensionKey,
        DateTimeOffset now) =>
        new()
        {
            UserId = userId,
            Track = track,
            Dimension = dimension,
            DimensionKey = dimensionKey,
            UpdatedAt = now,
        };

    /// <summary>Adds what <paramref name="attemptId"/> scored in this category.</summary>
    public void Add(Guid attemptId, SnapshotTally tally, DateTimeOffset now)
    {
        AvgTimeMs = SnapshotMaths.CombineAverage(AvgTimeMs, ItemsAnswered, tally.TimeMs, tally.Items);
        ItemsAnswered += tally.Items;
        ItemsCorrect += tally.ItemsCorrect;
        PointsEarned = ScoreMaths.Points(PointsEarned + tally.PointsEarned);
        MaxPoints = ScoreMaths.Points(MaxPoints + tally.MaxPoints);
        Accuracy = ScoreMaths.Share(PointsEarned, MaxPoints);
        AttemptsCount++;
        LastAttemptId = attemptId;
        UpdatedAt = now;
    }
}
