using System.Globalization;
using Nclex.Domain.Performance;

namespace Nclex.Domain.Tests.Performance;

public sealed class TrendBucketingTests
{
    private static readonly TimeZoneInfo LosAngeles = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles");
    private static readonly TimeZoneInfo Auckland = TimeZoneInfo.FindSystemTimeZoneById("Pacific/Auckland");
    private static readonly TimeZoneInfo NewYork = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");

    [Fact]
    public void A_sunday_night_test_in_los_angeles_counts_towards_that_week_although_it_is_monday_in_utc()
    {
        // Sunday 13 September 2026, 23:30 PDT.
        var instant = DateTimeOffset.Parse("2026-09-14T06:30:00Z", CultureInfo.InvariantCulture);

        TrendBucketing.BucketStart(instant, LosAngeles, TrendBucket.Week).ShouldBe(new DateOnly(2026, 9, 7));
        TrendBucketing.BucketStart(instant, TimeZoneInfo.Utc, TrendBucket.Week).ShouldBe(new DateOnly(2026, 9, 14));
    }

    [Fact]
    public void A_monday_morning_test_in_auckland_starts_a_new_week_although_it_is_sunday_in_utc()
    {
        // Monday 14 September 2026, 08:00 NZST.
        var instant = DateTimeOffset.Parse("2026-09-13T20:00:00Z", CultureInfo.InvariantCulture);

        TrendBucketing.BucketStart(instant, Auckland, TrendBucket.Week).ShouldBe(new DateOnly(2026, 9, 14));
        TrendBucketing.BucketStart(instant, TimeZoneInfo.Utc, TrendBucket.Week).ShouldBe(new DateOnly(2026, 9, 7));
    }

    [Fact]
    public void Day_buckets_follow_the_local_date()
    {
        var instant = DateTimeOffset.Parse("2026-09-14T06:30:00Z", CultureInfo.InvariantCulture);

        TrendBucketing.BucketStart(instant, LosAngeles, TrendBucket.Day).ShouldBe(new DateOnly(2026, 9, 13));
        TrendBucketing.BucketStart(instant, Auckland, TrendBucket.Day).ShouldBe(new DateOnly(2026, 9, 14));
    }

    [Fact]
    public void A_week_that_crosses_a_clock_change_stays_one_bucket()
    {
        // US clocks went forward on Sunday 8 March 2026; Monday 2 to Sunday 8 March is one week.
        var monday = DateTimeOffset.Parse("2026-03-02T05:00:00Z", CultureInfo.InvariantCulture);   // 00:00 EST
        var sunday = DateTimeOffset.Parse("2026-03-09T03:59:00Z", CultureInfo.InvariantCulture);   // 23:59 EDT

        TrendBucketing.BucketStart(monday, NewYork, TrendBucket.Week).ShouldBe(new DateOnly(2026, 3, 2));
        TrendBucketing.BucketStart(sunday, NewYork, TrendBucket.Week).ShouldBe(new DateOnly(2026, 3, 2));
    }

    [Fact]
    public void Days_start_at_local_midnight_with_the_offset_of_that_date()
    {
        TrendBucketing.StartOfDay(new DateOnly(2026, 3, 8), NewYork).ShouldBe(DateTimeOffset.Parse("2026-03-08T05:00:00Z", CultureInfo.InvariantCulture));
        TrendBucketing.StartOfDay(new DateOnly(2026, 3, 9), NewYork).ShouldBe(DateTimeOffset.Parse("2026-03-09T04:00:00Z", CultureInfo.InvariantCulture));
    }

    [Fact]
    public void A_day_whose_midnight_is_skipped_starts_at_the_first_local_time_that_exists()
    {
        // Santiago moves its clocks forward at midnight: 6 September 2026 starts at 01:00 local (UTC-3).
        var santiago = TimeZoneInfo.FindSystemTimeZoneById("America/Santiago");

        TrendBucketing.StartOfDay(new DateOnly(2026, 9, 6), santiago).ShouldBe(DateTimeOffset.Parse("2026-09-06T04:00:00Z", CultureInfo.InvariantCulture));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("Not/AZone")]
    public void An_unknown_time_zone_falls_back_to_utc(string? id) =>
        TrendBucketing.ZoneOrUtc(id).ShouldBe(TimeZoneInfo.Utc);
}
