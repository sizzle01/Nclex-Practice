using Nclex.Domain.Entities;

namespace Nclex.Application.Auth;

/// <summary>How roles are written in access tokens and authorization policies.</summary>
public static class RoleClaims
{
    /// <summary>The role as it appears in the "role" claim: the user_role enum value, e.g. "candidate".</summary>
    public static string ToClaimValue(this UserRole role) => role switch
    {
        UserRole.Candidate => "candidate",
        UserRole.Educator => "educator",
        UserRole.Author => "author",
        UserRole.Reviewer => "reviewer",
        UserRole.Admin => "admin",
        _ => throw new ArgumentOutOfRangeException(nameof(role), role, "Unknown role."),
    };
}
