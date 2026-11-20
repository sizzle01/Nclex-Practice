using Nclex.Domain.Entities;

namespace Nclex.Domain.Grading;

/// <summary>One item of an attempt in the order it was served, with what the clinical judgment results need.</summary>
/// <param name="Position">1-based position in the attempt; the first and second halves are split on it.</param>
/// <param name="Skill">The item's CJMM step; null for items without one (they still count in the halves).</param>
public sealed record ServedItem(int Position, decimal PointsEarned, decimal MaxPoints, long TimeSpentMs, CjmmSkill? Skill);

/// <summary>The score on one CJMM step.</summary>
/// <param name="ScorePct">Share of points, from 0 to 1, so partial credit counts as on the rest of the results page.</param>
/// <param name="AvgTimeMs">Average time per item on this step.</param>
public sealed record StepScore(CjmmSkill Skill, int Items, decimal PointsEarned, decimal MaxPoints, decimal ScorePct, int AvgTimeMs);

/// <summary>The score in one half of the attempt, overall and by step.</summary>
public sealed record HalfScore(int Items, decimal ScorePct, int AvgTimeMs, IReadOnlyList<StepScore> Steps);

/// <summary>
/// An attempt's clinical judgment results (F25): each CJMM step in model order, and the first half against the
/// second half, so a candidate can see whether judgment falls away as the exam goes on.
/// </summary>
/// <param name="SecondHalfDrop">True when the second half scored clearly lower than the first (see <see cref="ClinicalJudgment"/>).</param>
public sealed record ClinicalJudgmentResult(
    IReadOnlyList<StepScore> Steps,
    HalfScore FirstHalf,
    HalfScore SecondHalf,
    bool SecondHalfDrop);

/// <summary>
/// Works out clinical judgment results from the items an attempt served. Pure: the same items always give the
/// same result.
///
/// Why halves: the candidate's NCLEX reports showed the later reasoning steps falling below standard while early
/// steps held, a pattern that fits fatigue, pacing or rising anxiety. Comparing halves of one sitting makes that
/// pattern visible in practice.
/// </summary>
public static class ClinicalJudgment
{
    /// <summary>The NCSBN Clinical Judgment Measurement Model order, which the results page keeps.</summary>
    public static readonly IReadOnlyList<CjmmSkill> StepOrder =
    [
        CjmmSkill.RecognizeCues,
        CjmmSkill.AnalyzeCues,
        CjmmSkill.PrioritizeHypotheses,
        CjmmSkill.GenerateSolutions,
        CjmmSkill.TakeAction,
        CjmmSkill.EvaluateOutcomes,
    ];

    /// <summary>Fewer items than this in each half and a difference says more about chance than about stamina.</summary>
    public const int MinItemsPerHalfForDrop = 10;

    /// <summary>The fall, in percentage points, that counts as a clear drop in the second half.</summary>
    public const int DropPercentagePoints = 10;

    /// <summary>The results, or null when the attempt has fewer than two items and cannot be split.</summary>
    public static ClinicalJudgmentResult? For(IReadOnlyList<ServedItem> items)
    {
        if (items.Count < 2)
        {
            return null;
        }

        var ordered = items.OrderBy(i => i.Position).ToList();
        // The first half takes the odd item, so 85 items split 43 and 42.
        var firstCount = (ordered.Count + 1) / 2;
        var first = Half(ordered.Take(firstCount).ToList());
        var second = Half(ordered.Skip(firstCount).ToList());

        return new ClinicalJudgmentResult(Steps(ordered), first, second, IsClearDrop(first, second));
    }

    /// <summary>A clear drop: both halves big enough to compare, and the second at least 10 points lower.</summary>
    public static bool IsClearDrop(HalfScore first, HalfScore second) =>
        first.Items >= MinItemsPerHalfForDrop
        && second.Items >= MinItemsPerHalfForDrop
        && ScoreMaths.WholePercent(first.ScorePct) - ScoreMaths.WholePercent(second.ScorePct) >= DropPercentagePoints;

    private static HalfScore Half(List<ServedItem> items) =>
        new(items.Count, Share(items), AverageTime(items), Steps(items));

    // Only steps the items cover, in model order; items without a step are left out here.
    private static IReadOnlyList<StepScore> Steps(List<ServedItem> items) =>
        [.. StepOrder
            .Select(skill => (Skill: skill, Items: items.Where(i => i.Skill == skill).ToList()))
            .Where(step => step.Items.Count > 0)
            .Select(step => new StepScore(
                step.Skill,
                step.Items.Count,
                step.Items.Sum(i => i.PointsEarned),
                step.Items.Sum(i => i.MaxPoints),
                Share(step.Items),
                AverageTime(step.Items)))];

    private static decimal Share(List<ServedItem> items) =>
        ScoreMaths.Share(items.Sum(i => i.PointsEarned), items.Sum(i => i.MaxPoints));

    private static int AverageTime(List<ServedItem> items) =>
        items.Count == 0 ? 0 : (int)(items.Sum(i => i.TimeSpentMs) / items.Count);
}
