using Nclex.Domain.Entities;

namespace Nclex.Domain.Performance;

/// <summary>The dashboard's "weakest categories" across all attempts (interfaces.md 5.2, /dashboard).</summary>
public static class WeakestCategories
{
    public const int Count = 3;

    /// <summary>
    /// A category needs this many items before it can be called weak: one wrong answer on its own
    /// says little about a candidate.
    /// </summary>
    public const int MinItems = 3;

    /// <summary>
    /// The Client Needs categories with the lowest accuracy, worst first. On a tie the one with more
    /// items comes first, as in <see cref="Grading.WeakestAreas"/>.
    /// </summary>
    public static IReadOnlyList<PerformanceSnapshot> From(IEnumerable<PerformanceSnapshot> snapshots) =>
        [.. snapshots
            .Where(s => s.Dimension == PerformanceDimension.ClientNeeds && s.ItemsAnswered >= MinItems)
            .OrderBy(s => s.Accuracy)
            .ThenByDescending(s => s.ItemsAnswered)
            .ThenBy(s => s.DimensionKey, StringComparer.Ordinal)
            .Take(Count)];
}
