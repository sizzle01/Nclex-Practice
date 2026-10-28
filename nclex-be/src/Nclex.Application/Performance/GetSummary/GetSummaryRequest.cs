using Nclex.Domain.Entities;

namespace Nclex.Application.Performance.GetSummary;

/// <summary>Query string of GET /me/performance/summary (api.md section 5).</summary>
/// <param name="Track">RN or PN; the user's own track when left out.</param>
public sealed record GetSummaryRequest(ExamTrack? Track = null);
