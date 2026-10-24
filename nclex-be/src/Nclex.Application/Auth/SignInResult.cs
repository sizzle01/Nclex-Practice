using Nclex.Application.Auth.Sessions;
using Nclex.Application.Users;

namespace Nclex.Application.Auth;

/// <summary>What sign-up and login produce: the user's profile and their new session.</summary>
public sealed record SignInResult(UserProfile User, IssuedSession Session);

/// <summary>Body of a successful sign-up (201) or login (200), api.md section 2.</summary>
public sealed record AuthResponse(UserProfile User, string AccessToken, DateTimeOffset AccessTokenExpiresAt)
{
    public static AuthResponse From(SignInResult result) => new(
        result.User,
        result.Session.AccessToken.Value,
        result.Session.AccessToken.ExpiresAt);
}

/// <summary>Body of a successful refresh (api.md section 2, POST /auth/refresh).</summary>
public sealed record AccessTokenResponse(string AccessToken, DateTimeOffset AccessTokenExpiresAt)
{
    public static AccessTokenResponse From(IssuedSession session) => new(
        session.AccessToken.Value,
        session.AccessToken.ExpiresAt);
}
