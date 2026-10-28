using Nclex.Domain.Entities;

namespace Nclex.Domain.Grading;

/// <summary>The "focus on these" list of a result (api.md, GET /attempts/{id}/result <c>weakest</c>).</summary>
public static class WeakestAreas
{
    public const int Count = 3;

    /// <summary>
    /// The Client Needs categories with the lowest score, worst first. On a tie the one with more
    /// items comes first, because more evidence makes the weakness more certain.
    /// </summary>
    public static IReadOnlyList<AttemptCategoryScore> From(IEnumerable<AttemptCategoryScore> scores) =>
        [.. scores
            .Where(s => s.Dimension == ScoreDimension.ClientNeeds)
            .OrderBy(s => s.Pct)
            .ThenByDescending(s => s.Items)
            .ThenBy(s => s.DimensionKey, StringComparer.Ordinal)
            .Take(Count)];
}
