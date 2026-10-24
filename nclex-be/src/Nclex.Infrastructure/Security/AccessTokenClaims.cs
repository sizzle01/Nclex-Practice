namespace Nclex.Infrastructure.Security;

/// <summary>Claim names inside an access token. Kept short and standard; no personal data.</summary>
public static class AccessTokenClaims
{
    /// <summary>The user's id (JWT "sub").</summary>
    public const string Subject = "sub";

    /// <summary>The user's role, e.g. "candidate" (matches the user_role enum).</summary>
    public const string Role = "role";

    /// <summary>Unique token id (JWT "jti").</summary>
    public const string TokenId = "jti";
}
