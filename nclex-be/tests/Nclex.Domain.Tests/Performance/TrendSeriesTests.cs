using System.Globalization;
using Nclex.Domain.Performance;

namespace Nclex.Domain.Tests.Performance;

public sealed class TrendSeriesTests
{
    [Fact]
    public void Attempts_of_the_same_week_are_added_up_by_points_and_items()
    {
        var samples = new[]
        {
            Sample("2026-09-15T10:00:00Z", items: 4, earned: 3, max: 4, timeMs: 4000),
            Sample("2026-09-17T10:00:00Z", items: 6, earned: 2, max: 6, timeMs: 9000),
            Sample("2026-09-08T10:00:00Z", items: 2, earned: 2, max: 2, timeMs: 1000),
        };

        var series = TrendSeries.Build(samples, TimeZoneInfo.Utc, TrendBucket.Week);

        series.ShouldBe(
        [
            new TrendPoint(new DateOnly(2026, 9, 7), 2, 1.0000m, 500),
            new TrendPoint(new DateOnly(2026, 9, 14), 10, 0.5000m, 1300),
        ]);
    }

    [Fact]
    public void Weeks_without_attempts_are_left_out_rather_than_shown_as_zero()
    {
        var samples = new[]
        {
            Sample("2026-09-01T10:00:00Z", items: 1, earned: 1, max: 1, timeMs: 100),
            Sample("2026-09-29T10:00:00Z", items: 1, earned: 0, max: 1, timeMs: 100),
        };

        TrendSeries.Build(samples, TimeZoneInfo.Utc, TrendBucket.Week).Select(p => p.BucketStart)
            .ShouldBe([new DateOnly(2026, 8, 31), new DateOnly(2026, 9, 28)]);
    }

    [Fact]
    public void No_attempts_give_an_empty_series() =>
        TrendSeries.Build([], TimeZoneInfo.Utc, TrendBucket.Day).ShouldBeEmpty();

    private static TrendSample Sample(string at, int items, decimal earned, decimal max, long timeMs) =>
        new(DateTimeOffset.Parse(at, CultureInfo.InvariantCulture), items, earned, max, timeMs);
}
