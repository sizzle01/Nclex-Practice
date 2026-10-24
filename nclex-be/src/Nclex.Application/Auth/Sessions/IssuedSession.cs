namespace Nclex.Application.Auth.Sessions;

/// <summary>
/// Credentials handed to a client when it signs in or refreshes. The access token goes in the
/// response body; the refresh token only ever goes in the HttpOnly cookie.
/// </summary>
public sealed record IssuedSession(Abstractions.AccessToken AccessToken, RefreshToken RefreshToken);

/// <summary>An opaque refresh token for the cookie, and when it expires (the cookie's Max-Age).</summary>
public sealed record RefreshToken(string Value, DateTimeOffset ExpiresAt);
