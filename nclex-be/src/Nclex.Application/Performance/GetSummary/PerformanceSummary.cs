using Nclex.Domain.Entities;

namespace Nclex.Application.Performance.GetSummary;

/// <summary>
/// Body of GET /me/performance/summary: the user's totals in a track, for the dashboard. All zero
/// before the first submitted attempt. The streak in api.md waits for the phase 2 streaks feature.
/// </summary>
/// <param name="ItemsAnswered">Items served across every attempt, answered or not.</param>
/// <param name="Accuracy">Points earned as a share of the maximum, 0 to 1 (partial credit counts).</param>
/// <param name="AvgTimeMs">Average time per item.</param>
/// <param name="Weakest">Up to three Client Needs categories with the lowest accuracy, worst first.</param>
public sealed record PerformanceSummary(
    ExamTrack Track,
    int AttemptsCount,
    int ItemsAnswered,
    int ItemsCorrect,
    decimal Accuracy,
    int AvgTimeMs,
    IReadOnlyList<BreakdownRow> Weakest);
