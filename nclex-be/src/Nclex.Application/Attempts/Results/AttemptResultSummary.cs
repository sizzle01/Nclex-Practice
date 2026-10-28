using Nclex.Domain.Entities;

namespace Nclex.Application.Attempts.Results;

/// <summary>The headline of a result: score, time and readiness (api.md, POST /submit <c>result</c>).</summary>
/// <param name="Readiness">Null when the set has no pass threshold.</param>
public sealed record AttemptResultSummary(ResultScore Score, ResultTime Time, ResultReadiness? Readiness)
{
    public static AttemptResultSummary From(AttemptResult result) => new(
        new ResultScore(
            result.PointsEarned,
            result.MaxPoints,
            result.ScorePct,
            result.ItemsCorrect,
            result.ItemsAnswered,
            result.ItemsTotal),
        new ResultTime(result.TotalTimeMs, result.AvgTimeMs),
        result is { PassThreshold: { } threshold, Passed: { } passed, ReadinessLabel: { } label }
            ? new ResultReadiness(threshold, passed, label, ResultReadiness.EstimateDisclaimer)
            : null);
}

/// <param name="Pct">Points earned as a share of the maximum, 0 to 1; partial credit counts.</param>
/// <param name="ItemsCorrect">Items that earned full marks.</param>
public sealed record ResultScore(
    decimal PointsEarned,
    decimal MaxPoints,
    decimal Pct,
    int ItemsCorrect,
    int ItemsAnswered,
    int ItemsTotal);

/// <param name="TotalMs">Time on items, as the server counted it.</param>
public sealed record ResultTime(long TotalMs, int AvgPerItemMs);

/// <summary>The readiness estimate. Always sent with its disclaimer, which the UI shows beside it (architecture.md 5.5).</summary>
public sealed record ResultReadiness(decimal PassThreshold, bool Passed, ReadinessLabel Label, string Disclaimer)
{
    public const string EstimateDisclaimer =
        "This is an estimate based on your practice score, not a prediction of your NCLEX result.";
}
