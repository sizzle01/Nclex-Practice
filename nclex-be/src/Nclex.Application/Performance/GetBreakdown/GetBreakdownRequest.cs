using Nclex.Domain.Entities;
using Nclex.Domain.Performance;

namespace Nclex.Application.Performance.GetBreakdown;

/// <summary>Query string of GET /me/performance/breakdown (api.md section 5).</summary>
/// <param name="Dimension">The slicing to show, e.g. "client_needs"; not "overall", which has one row.</param>
/// <param name="Track">RN or PN; the user's own track when left out.</param>
public sealed record GetBreakdownRequest(PerformanceDimension? Dimension = null, ExamTrack? Track = null);
