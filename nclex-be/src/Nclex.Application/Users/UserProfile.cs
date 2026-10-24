using Nclex.Domain.Entities;

namespace Nclex.Application.Users;

/// <summary>
/// The signed-in user's own profile, as returned by sign-up, login, GET /me and PATCH /me.
/// It is the api.md section 2 "user" object plus the editable fields timezone and targetExamDate.
/// </summary>
public sealed record UserProfile(
    Guid Id,
    string Email,
    bool EmailVerified,
    UserRole Role,
    ExamTrack Track,
    PlanTier PlanTier,
    string? DisplayName,
    string Timezone,
    DateOnly? TargetExamDate)
{
    public static UserProfile From(User user) => new(
        user.Id,
        user.Email,
        EmailVerified: user.EmailVerifiedAt is not null,
        user.Role,
        user.Track,
        user.PlanTier,
        user.DisplayName,
        user.Timezone,
        user.TargetExamDate);
}
