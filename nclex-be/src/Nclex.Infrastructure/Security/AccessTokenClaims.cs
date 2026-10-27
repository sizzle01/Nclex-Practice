namespace Nclex.Infrastructure.Security;

/// <summary>Claim names inside an access token. Kept short and standard; no personal data.</summary>
public static class AccessTokenClaims
{
    /// <summary>The user's id (JWT "sub").</summary>
    public const string Subject = "sub";

    /// <summary>The user's role, e.g. "candidate" (matches the user_role enum).</summary>
    public const string Role = "role";

    /// <summary>
    /// The refresh-session family (one sign-in on one device) the token belongs to, so account
    /// changes can keep this device signed in while signing out the others.
    /// </summary>
    public const string SessionId = "sid";

    /// <summary>Unique token id (JWT "jti").</summary>
    public const string TokenId = "jti";
}
