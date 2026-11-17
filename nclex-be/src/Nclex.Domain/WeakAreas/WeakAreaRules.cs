using Nclex.Domain.Grading;

namespace Nclex.Domain.WeakAreas;

/// <summary>
/// One attempt's score in one Client Needs leaf: a row of <c>attempt_category_scores</c> with
/// dimension <c>client_needs</c>.
/// </summary>
/// <param name="Code">The leaf's code, e.g. "PHI.PPT".</param>
/// <param name="Items">Items served in the leaf, answered or not (the results breakdown's count).</param>
/// <param name="PointsEarned">Points earned, partial credit included.</param>
/// <param name="MaxPoints">The most the items could earn.</param>
public sealed record LeafScore(string Code, int Items, decimal PointsEarned, decimal MaxPoints);

/// <summary>A Client Needs leaf the candidate is weak in, across their recent attempts.</summary>
/// <param name="Code">The leaf's code.</param>
/// <param name="Items">Items served in the leaf across the recent attempts.</param>
/// <param name="Pct">Points earned as a share of the maximum, 0 to 1.</param>
public sealed record WeakArea(string Code, int Items, decimal Pct);

/// <summary>
/// Which Client Needs leaves count as weak for "Practice your weak areas" (architecture.md 5.7.1).
/// The rule looks at recent attempts only, so an area the candidate has since improved in stops
/// being called weak; all-time snapshots would remember old mistakes for ever.
/// </summary>
public static class WeakAreaRules
{
    /// <summary>How many of the latest submitted attempts on the track are looked at.</summary>
    public const int RecentAttempts = 10;

    /// <summary>A leaf is weak below this share, the same as the default pass threshold.</summary>
    public const decimal Threshold = 0.65m;

    /// <summary>
    /// A leaf needs this many items before it can be called weak: missing 2 of 3 says too little.
    /// </summary>
    public const int MinItems = 5;

    /// <summary>
    /// The weak leaves, worst first. On a tie the leaf with more items comes first (more evidence),
    /// then the code, so the order never depends on the database. An empty list means nothing
    /// qualifies, and the caller says so rather than inventing a weakness.
    /// </summary>
    public static IReadOnlyList<WeakArea> From(IEnumerable<LeafScore> scores) =>
        [.. scores
            .GroupBy(s => s.Code, StringComparer.Ordinal)
            .Select(g => new WeakArea(
                g.Key,
                g.Sum(s => s.Items),
                ScoreMaths.Share(g.Sum(s => s.PointsEarned), g.Sum(s => s.MaxPoints))))
            .Where(a => a.Items >= MinItems && a.Pct < Threshold)
            .OrderBy(a => a.Pct)
            .ThenByDescending(a => a.Items)
            .ThenBy(a => a.Code, StringComparer.Ordinal)];
}
