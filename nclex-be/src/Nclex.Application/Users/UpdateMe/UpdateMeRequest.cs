using Nclex.Domain.Entities;

namespace Nclex.Application.Users.UpdateMe;

/// <summary>
/// Body of PATCH /me (api.md section 2). Every field is optional: an omitted or null field is
/// left unchanged. Send an empty displayName to remove it.
/// </summary>
/// <param name="DisplayName">Name shown in the app, at most 80 characters.</param>
/// <param name="Track">RN or PN.</param>
/// <param name="Timezone">IANA time zone id, e.g. "America/Chicago".</param>
/// <param name="TargetExamDate">The day the candidate plans to sit the real exam.</param>
public sealed record UpdateMeRequest(
    string? DisplayName = null,
    ExamTrack? Track = null,
    string? Timezone = null,
    DateOnly? TargetExamDate = null);
