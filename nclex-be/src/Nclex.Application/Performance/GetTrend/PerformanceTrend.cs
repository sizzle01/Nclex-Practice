using Nclex.Domain.Entities;
using Nclex.Domain.Performance;

namespace Nclex.Application.Performance.GetTrend;

/// <summary>Body of GET /me/performance/trend: one point per day or week with attempts, oldest first.</summary>
/// <param name="Key">The category followed; "all" for the overall trend.</param>
public sealed record PerformanceTrend(
    ExamTrack Track,
    PerformanceDimension Dimension,
    string Key,
    TrendBucket Bucket,
    IReadOnlyList<TrendPoint> Series);
