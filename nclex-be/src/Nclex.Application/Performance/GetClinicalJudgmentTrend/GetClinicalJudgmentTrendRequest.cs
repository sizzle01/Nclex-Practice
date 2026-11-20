using Nclex.Domain.Entities;

namespace Nclex.Application.Performance.GetClinicalJudgmentTrend;

/// <summary>Query string of GET /me/performance/clinical-judgment (api.md section 5, F25).</summary>
/// <param name="Track">RN or PN; the user's own track when left out.</param>
public sealed record GetClinicalJudgmentTrendRequest(ExamTrack? Track = null);
