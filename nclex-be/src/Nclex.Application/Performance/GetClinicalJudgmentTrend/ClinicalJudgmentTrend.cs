using Nclex.Domain.Entities;

namespace Nclex.Application.Performance.GetClinicalJudgmentTrend;

/// <summary>
/// Each CJMM step across the user's most recent timed attempts (F25), laid out like the candidate's NCLEX
/// performance report: steps down the side, attempts across, oldest first.
/// </summary>
/// <param name="Attempts">Up to <see cref="GetClinicalJudgmentTrendHandler.RecentAttempts"/> timed attempts, oldest first.</param>
/// <param name="Steps">All six steps in model order.</param>
public sealed record ClinicalJudgmentTrend(ExamTrack Track, IReadOnlyList<TrendAttempt> Attempts, IReadOnlyList<StepTrend> Steps);

/// <summary>One column of the grid.</summary>
public sealed record TrendAttempt(Guid AttemptId, string Title, DateTimeOffset SubmittedAt);

/// <summary>One row of the grid.</summary>
/// <param name="Scores">The step's share of points in each attempt, in the same order as the attempts; null when the attempt had no item on this step.</param>
public sealed record StepTrend(string Key, string Name, IReadOnlyList<decimal?> Scores);
