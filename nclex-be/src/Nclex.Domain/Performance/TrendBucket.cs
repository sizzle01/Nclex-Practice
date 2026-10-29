namespace Nclex.Domain.Performance;

/// <summary>The width of one point on a trend chart (api.md section 5, <c>bucket</c>).</summary>
public enum TrendBucket
{
    Day,

    /// <summary>A week starting on Monday (ISO 8601).</summary>
    Week,
}
