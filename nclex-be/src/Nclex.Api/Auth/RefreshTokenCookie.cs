using Nclex.Application.Auth.Sessions;

namespace Nclex.Api.Auth;

/// <summary>
/// Reads and writes the refresh-token cookie (api.md section 2, architecture.md section 6).
/// HttpOnly keeps it away from JavaScript; SameSite=Strict and the /api/v1/auth path mean the
/// browser only sends it to the auth endpoints, and only from our own pages (CSRF protection).
/// </summary>
public static class RefreshTokenCookie
{
    public const string Name = "rt";
    public const string Path = "/api/v1/auth";

    /// <summary>Header the refresh endpoint requires, as a second CSRF guard (api.md section 2).</summary>
    public const string RequestedWithHeader = "X-Requested-With";
    public const string RequestedWithValue = "fetch";

    public static string? Read(HttpRequest request) => request.Cookies[Name];

    public static void Write(HttpResponse response, RefreshToken token, DateTimeOffset now) =>
        // Rounded so a fresh token reads as exactly 30 days (Max-Age=2592000) rather than a second less.
        response.Cookies.Append(Name, token.Value, Options(maxAge: TimeSpan.FromSeconds(Math.Round((token.ExpiresAt - now).TotalSeconds))));

    public static void Delete(HttpResponse response) =>
        response.Cookies.Delete(Name, Options(maxAge: null));

    private static CookieOptions Options(TimeSpan? maxAge) => new()
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Strict,
        Path = Path,
        MaxAge = maxAge,
        IsEssential = true,
    };
}
