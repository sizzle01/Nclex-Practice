using Nclex.Domain.Grading;

namespace Nclex.Application.Attempts.Results;

/// <summary>
/// The clinical judgment section of a result (F25, api.md section 4): each CJMM step in model order, and the
/// first half of the attempt against the second, with time per question.
/// </summary>
/// <param name="SecondHalfDrop">True when the second half scored at least 10 points lower than the first, with at least 10 items in each half.</param>
public sealed record ClinicalJudgmentView(
    IReadOnlyList<StepResult> Steps,
    HalfResult FirstHalf,
    HalfResult SecondHalf,
    bool SecondHalfDrop)
{
    public static ClinicalJudgmentView From(ClinicalJudgmentResult result, Func<string, string> stepName) => new(
        Map(result.Steps, stepName),
        Half(result.FirstHalf, stepName),
        Half(result.SecondHalf, stepName),
        result.SecondHalfDrop);

    private static HalfResult Half(HalfScore half, Func<string, string> stepName) =>
        new(half.Items, half.ScorePct, half.AvgTimeMs, Map(half.Steps, stepName));

    private static IReadOnlyList<StepResult> Map(IReadOnlyList<StepScore> steps, Func<string, string> stepName) =>
        [.. steps.Select(s =>
        {
            var key = DimensionKeys.For(s.Skill);
            return new StepResult(key, stepName(key), s.Items, s.PointsEarned, s.MaxPoints, s.ScorePct, s.AvgTimeMs);
        })];
}

/// <summary>The score on one CJMM step.</summary>
/// <param name="Key">The snake_case step, e.g. "take_action", as in the cjmm_skill breakdown.</param>
/// <param name="Pct">Share of points, 0 to 1.</param>
/// <param name="AvgTimeMs">Average time per question on this step.</param>
public sealed record StepResult(string Key, string Name, int Items, decimal PointsEarned, decimal MaxPoints, decimal Pct, int AvgTimeMs);

/// <summary>One half of the attempt, overall and by step.</summary>
/// <param name="Pct">Share of points, 0 to 1.</param>
/// <param name="AvgTimeMs">Average time per question in this half.</param>
public sealed record HalfResult(int Items, decimal Pct, int AvgTimeMs, IReadOnlyList<StepResult> Steps);
