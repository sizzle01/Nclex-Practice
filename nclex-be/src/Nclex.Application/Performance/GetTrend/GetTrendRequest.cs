using Nclex.Domain.Entities;
using Nclex.Domain.Performance;

namespace Nclex.Application.Performance.GetTrend;

/// <summary>Query string of GET /me/performance/trend (api.md section 5).</summary>
/// <param name="Dimension">"overall" (the default) or a breakdown dimension such as "client_needs".</param>
/// <param name="Key">The category to follow, e.g. "PI.PPT"; required unless the dimension is overall.</param>
/// <param name="Bucket">"week" (the default, weeks start on Monday) or "day", in the user's time zone.</param>
/// <param name="From">First local date to include; from the first attempt when left out.</param>
/// <param name="To">Last local date to include; up to now when left out.</param>
/// <param name="Track">RN or PN; the user's own track when left out.</param>
public sealed record GetTrendRequest(
    PerformanceDimension? Dimension = null,
    string? Key = null,
    TrendBucket? Bucket = null,
    DateOnly? From = null,
    DateOnly? To = null,
    ExamTrack? Track = null);
