using Nclex.Domain.Entities;

namespace Nclex.Application.WeakAreas.GetWeakAreas;

/// <summary>Query string of GET /me/weak-areas (api.md section 5).</summary>
/// <param name="Track">RN or PN; the user's own track when left out.</param>
public sealed record GetWeakAreasRequest(ExamTrack? Track = null);
