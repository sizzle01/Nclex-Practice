using Nclex.Domain.Grading;

namespace Nclex.Domain.Entities;

/// <summary>
/// The score of a submitted attempt, written once in the submit transaction. Maps to
/// <c>attempt_results</c> (database.md section 2.7); its breakdown rows are
/// <see cref="AttemptCategoryScore"/>.
/// </summary>
public sealed class AttemptResult
{
    // For EF Core, which sets properties directly when loading rows.
    private AttemptResult()
    {
    }

    public Guid AttemptId { get; private set; }

    public decimal PointsEarned { get; private set; }

    public decimal MaxPoints { get; private set; }

    /// <summary>Points earned as a share of the maximum, 0 to 1, rounded to 4 places.</summary>
    public decimal ScorePct { get; private set; }

    /// <summary>Items that earned full marks.</summary>
    public int ItemsCorrect { get; private set; }

    public int ItemsAnswered { get; private set; }

    public int ItemsTotal { get; private set; }

    /// <summary>Time on items, as the server counted it (architecture.md 5.2).</summary>
    public long TotalTimeMs { get; private set; }

    public int AvgTimeMs { get; private set; }

    /// <summary>The set's threshold when the attempt started; null when it has none.</summary>
    public decimal? PassThreshold { get; private set; }

    /// <summary>Null when there is no threshold.</summary>
    public bool? Passed { get; private set; }

    public ReadinessLabel? ReadinessLabel { get; private set; }

    /// <summary>Adaptive-testing ability estimate (phase 2).</summary>
    public decimal? CatTheta { get; private set; }

    /// <summary>Standard error of <see cref="CatTheta"/> (phase 2).</summary>
    public decimal? CatSe { get; private set; }

    public DateTimeOffset GradedAt { get; private set; }

    /// <summary>The result of <paramref name="attemptId"/> from its totals and readiness estimate.</summary>
    public static AttemptResult Create(
        Guid attemptId,
        ResultTotals totals,
        decimal? passThreshold,
        Readiness readiness,
        DateTimeOffset gradedAt) =>
        new()
        {
            AttemptId = attemptId,
            PointsEarned = ScoreMaths.Points(totals.PointsEarned),
            MaxPoints = ScoreMaths.Points(totals.MaxPoints),
            ScorePct = totals.ScorePct,
            ItemsCorrect = totals.ItemsCorrect,
            ItemsAnswered = totals.ItemsAnswered,
            ItemsTotal = totals.ItemsTotal,
            TotalTimeMs = totals.TotalTimeMs,
            AvgTimeMs = totals.AvgTimeMs,
            PassThreshold = passThreshold is { } threshold ? ScoreMaths.Share(threshold) : null,
            Passed = readiness.Passed,
            ReadinessLabel = readiness.Label,
            GradedAt = gradedAt,
        };
}
