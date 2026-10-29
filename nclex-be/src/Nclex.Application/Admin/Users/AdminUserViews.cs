using Nclex.Domain.Entities;

namespace Nclex.Application.Admin.Users;

/// <summary>An account as the admin user list shows it.</summary>
/// <param name="LockedUntil">Set while logins are refused after repeated failures.</param>
public sealed record AdminUserView(
    Guid Id,
    string Email,
    string? DisplayName,
    UserRole Role,
    PlanTier PlanTier,
    ExamTrack Track,
    bool EmailVerified,
    DateTimeOffset? LockedUntil,
    DateTimeOffset CreatedAt);

/// <summary>Body of GET /admin/users.</summary>
/// <param name="NextCursor">Pass as <c>cursor</c> for the next page; null on the last page.</param>
public sealed record AdminUserPage(IReadOnlyList<AdminUserView> Items, string? NextCursor);
