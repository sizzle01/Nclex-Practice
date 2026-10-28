using Nclex.Domain.Grading;

namespace Nclex.Domain.Entities;

/// <summary>
/// One row of an attempt's score breakdown, e.g. "Management of Care: 8 of 12". Stored at submit
/// so the results page is a single indexed read (architecture.md 5.5). Maps to
/// <c>attempt_category_scores</c> (database.md section 2.7).
/// </summary>
public sealed class AttemptCategoryScore
{
    // For EF Core, which sets properties directly when loading rows.
    private AttemptCategoryScore()
    {
    }

    public Guid AttemptId { get; private set; }

    public ScoreDimension Dimension { get; private set; }

    /// <summary>A Client Needs code, body-system slug, item type, CJMM step or difficulty, in snake_case.</summary>
    public string DimensionKey { get; private set; } = string.Empty;

    public int Items { get; private set; }

    public int ItemsCorrect { get; private set; }

    public decimal PointsEarned { get; private set; }

    public decimal MaxPoints { get; private set; }

    public long TimeMs { get; private set; }

    /// <summary>Points earned as a share of the maximum; 0 when nothing could be earned.</summary>
    public decimal Pct => ScoreMaths.Share(PointsEarned, MaxPoints);

    public static AttemptCategoryScore From(Guid attemptId, CategoryTally tally) =>
        new()
        {
            AttemptId = attemptId,
            Dimension = tally.Dimension,
            DimensionKey = tally.Key,
            Items = tally.Items,
            ItemsCorrect = tally.ItemsCorrect,
            PointsEarned = tally.PointsEarned,
            MaxPoints = tally.MaxPoints,
            TimeMs = tally.TimeMs,
        };
}
