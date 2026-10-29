namespace Nclex.Domain.Performance;

/// <summary>
/// Puts attempts into the days or weeks of a trend chart. Buckets follow the user's own calendar
/// (users.timezone), so a test finished late on Sunday evening counts towards that week, not the
/// next one as it would in UTC.
/// </summary>
public static class TrendBucketing
{
    /// <summary>The local date that starts the bucket holding <paramref name="instant"/>.</summary>
    public static DateOnly BucketStart(DateTimeOffset instant, TimeZoneInfo zone, TrendBucket bucket)
    {
        var localDate = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(instant, zone).DateTime);
        return bucket == TrendBucket.Day ? localDate : StartOfWeek(localDate);
    }

    /// <summary>
    /// The UTC instant at which <paramref name="date"/> starts in <paramref name="zone"/>. Used to turn
    /// the chart's from/to dates into a query range. If a clock change skips local midnight, the day
    /// starts at the first instant that exists.
    /// </summary>
    public static DateTimeOffset StartOfDay(DateOnly date, TimeZoneInfo zone)
    {
        var midnight = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);
        while (zone.IsInvalidTime(midnight))
        {
            midnight = midnight.AddMinutes(30);
        }

        return new DateTimeOffset(midnight, zone.GetUtcOffset(midnight)).ToUniversalTime();
    }

    /// <summary>The user's time zone, or UTC when the stored id is not one this server knows.</summary>
    public static TimeZoneInfo ZoneOrUtc(string? ianaId) =>
        ianaId is not null && TimeZoneInfo.TryFindSystemTimeZoneById(ianaId, out var zone) ? zone : TimeZoneInfo.Utc;

    private static DateOnly StartOfWeek(DateOnly date)
    {
        // DayOfWeek counts from Sunday = 0; ISO weeks start on Monday.
        var daysSinceMonday = ((int)date.DayOfWeek + 6) % 7;
        return date.AddDays(-daysSinceMonday);
    }
}
