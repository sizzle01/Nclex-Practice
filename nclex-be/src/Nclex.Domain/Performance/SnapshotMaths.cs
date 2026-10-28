namespace Nclex.Domain.Performance;

/// <summary>
/// Running totals for <c>performance_snapshots</c>. The table keeps an average time, not a total,
/// so each attempt is folded into the average it already has.
/// </summary>
public static class SnapshotMaths
{
    /// <summary>
    /// The average time per item once <paramref name="addedItems"/> taking <paramref name="addedTimeMs"/>
    /// in all join <paramref name="items"/> that averaged <paramref name="averageMs"/>. Rounded to whole
    /// milliseconds; a rebuild replays attempts in the same order, so it rounds the same way.
    /// </summary>
    public static int CombineAverage(int averageMs, int items, long addedTimeMs, int addedItems)
    {
        var totalItems = items + addedItems;
        if (totalItems <= 0)
        {
            return 0;
        }

        var totalTimeMs = ((decimal)averageMs * items) + addedTimeMs;
        return (int)Math.Round(totalTimeMs / totalItems, MidpointRounding.AwayFromZero);
    }
}
