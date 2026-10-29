using Nclex.Domain.Entities;

namespace Nclex.Application.Admin.Users;

/// <summary>Query string of GET /admin/users.</summary>
/// <param name="Q">Part of an email or display name.</param>
public sealed record ListAdminUsersRequest(string? Q = null, int? Limit = null, string? Cursor = null)
{
    public const int MaxQueryLength = 200;
}

/// <summary>Body of PATCH /admin/users/{id}. Fields left out stay as they are.</summary>
/// <param name="Unlock">True lifts a login lock now instead of waiting for it to expire.</param>
public sealed record UpdateAdminUserRequest(UserRole? Role, PlanTier? PlanTier, bool? Unlock);
