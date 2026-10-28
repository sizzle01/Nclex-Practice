using Nclex.Domain.Entities;

namespace Nclex.Domain.Grading;

/// <summary>
/// Works out an attempt's totals and its breakdown by every dimension (architecture.md 5.5). Pure:
/// the same graded items always give the same result.
/// </summary>
public static class ResultCalculator
{
    public static ResultTotals Totals(IReadOnlyList<GradedItem> items)
    {
        if (items.Count == 0)
        {
            throw new ArgumentException("An attempt always has at least one item.", nameof(items));
        }

        var earned = items.Sum(i => i.PointsEarned);
        var max = items.Sum(i => i.MaxPoints);
        var totalTimeMs = items.Sum(i => i.TimeSpentMs);
        return new ResultTotals(
            earned,
            max,
            ScoreMaths.Share(earned, max),
            items.Count(i => i.IsCorrect),
            items.Count(i => i.IsAnswered),
            items.Count,
            totalTimeMs,
            // Per item served, so skipped items pull the average down the way they do on the clock.
            (int)(totalTimeMs / items.Count));
    }

    /// <summary>One tally per key of every dimension. Items without a CJMM step are left out of that dimension.</summary>
    public static IReadOnlyList<CategoryTally> Breakdown(IReadOnlyList<GradedItem> items) =>
    [
        .. Tally(items, ScoreDimension.ClientNeeds, t => t.ClientNeedsCode),
        .. Tally(items, ScoreDimension.ClientNeedsParent, t => t.ClientNeedsParentCode),
        .. Tally(items, ScoreDimension.BodySystem, t => t.BodySystemSlug),
        .. Tally(items, ScoreDimension.ItemType, t => DimensionKeys.For(t.ItemType)),
        .. Tally(items, ScoreDimension.CjmmSkill, t => t.CjmmSkill is { } skill ? DimensionKeys.For(skill) : null),
        .. Tally(items, ScoreDimension.Difficulty, t => DimensionKeys.For(t.Difficulty)),
    ];

    private static IEnumerable<CategoryTally> Tally(
        IReadOnlyList<GradedItem> items,
        ScoreDimension dimension,
        Func<ItemTags, string?> keyOf) =>
        items
            .Select(item => (Item: item, Key: keyOf(item.Tags)))
            .Where(x => x.Key is not null)
            .GroupBy(x => x.Key!, StringComparer.Ordinal)
            .OrderBy(group => group.Key, StringComparer.Ordinal)
            .Select(group => new CategoryTally(
                dimension,
                group.Key,
                group.Count(),
                group.Count(x => x.Item.IsCorrect),
                group.Sum(x => x.Item.PointsEarned),
                group.Sum(x => x.Item.MaxPoints),
                group.Sum(x => x.Item.TimeSpentMs)));
}
