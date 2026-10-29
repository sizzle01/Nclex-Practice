using Nclex.Domain.Grading;

namespace Nclex.Domain.Performance;

/// <summary>What one attempt scored in the category a trend chart follows.</summary>
public sealed record TrendSample(DateTimeOffset SubmittedAt, int Items, decimal PointsEarned, decimal MaxPoints, long TimeMs);

/// <summary>One point of a trend chart (api.md section 5, GET /me/performance/trend).</summary>
/// <param name="BucketStart">The local date the day or week starts.</param>
/// <param name="Pct">Points earned as a share of the maximum, 0 to 1, as on the results page.</param>
/// <param name="AvgTimeMs">Average time per item served.</param>
public sealed record TrendPoint(DateOnly BucketStart, int Items, decimal Pct, int AvgTimeMs);

/// <summary>Turns per-attempt samples into a trend chart's series. Pure: no clock, no database.</summary>
public static class TrendSeries
{
    /// <summary>
    /// One point per day or week that has at least one attempt, oldest first. Empty buckets are left
    /// out rather than shown as 0%, which would read as a score.
    /// </summary>
    public static IReadOnlyList<TrendPoint> Build(IEnumerable<TrendSample> samples, TimeZoneInfo zone, TrendBucket bucket) =>
        [.. samples
            .GroupBy(s => TrendBucketing.BucketStart(s.SubmittedAt, zone, bucket))
            .OrderBy(g => g.Key)
            .Select(g =>
            {
                var items = g.Sum(s => s.Items);
                var timeMs = g.Sum(s => s.TimeMs);
                return new TrendPoint(
                    g.Key,
                    items,
                    ScoreMaths.Share(g.Sum(s => s.PointsEarned), g.Sum(s => s.MaxPoints)),
                    items == 0 ? 0 : (int)(timeMs / items));
            })];
}
