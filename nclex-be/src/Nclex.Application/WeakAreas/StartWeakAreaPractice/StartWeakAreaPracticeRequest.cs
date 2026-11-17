using Nclex.Domain.Entities;

namespace Nclex.Application.WeakAreas.StartWeakAreaPractice;

/// <summary>Body of POST /test-sets/weak-areas (api.md section 3).</summary>
/// <param name="Track">RN or PN; the user's own track when left out.</param>
public sealed record StartWeakAreaPracticeRequest(ExamTrack? Track = null);
