namespace Nclex.Domain.Grading;

/// <summary>The headline numbers of a result (the score columns of <c>attempt_results</c>).</summary>
public sealed record ResultTotals(
    decimal PointsEarned,
    decimal MaxPoints,
    decimal ScorePct,
    int ItemsCorrect,
    int ItemsAnswered,
    int ItemsTotal,
    long TotalTimeMs,
    int AvgTimeMs);

/// <summary>The score of one slice of an attempt, e.g. every Management of Care item.</summary>
public sealed record CategoryTally(
    Entities.ScoreDimension Dimension,
    string Key,
    int Items,
    int ItemsCorrect,
    decimal PointsEarned,
    decimal MaxPoints,
    long TimeMs);
