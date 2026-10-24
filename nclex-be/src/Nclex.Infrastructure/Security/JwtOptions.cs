namespace Nclex.Infrastructure.Security;

/// <summary>Settings for signing and checking access tokens, bound from the "Jwt" configuration section.</summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    /// <summary>The HMAC-SHA256 key needs at least 256 bits.</summary>
    public const int MinimumSigningKeyBytes = 32;

    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    /// <summary>Secret key. Comes from secrets in deployed environments, never from appsettings.json.</summary>
    public string SigningKey { get; set; } = string.Empty;

    /// <summary>Access token lifetime: 15 minutes (architecture.md section 6).</summary>
    public TimeSpan AccessTokenLifetime { get; set; } = TimeSpan.FromMinutes(15);
}
