using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Nclex.Application.Abstractions;
using Nclex.Application.Auth;
using Nclex.Domain.Entities;

namespace Nclex.Infrastructure.Security;

/// <summary>Signs short-lived access tokens (JWT with HMAC-SHA256).</summary>
public sealed class JwtAccessTokenIssuer(IOptions<JwtOptions> options, TimeProvider clock) : IAccessTokenIssuer
{
    private readonly JsonWebTokenHandler _handler = new();

    public AccessToken Issue(User user, Guid sessionFamilyId)
    {
        var settings = options.Value;
        var now = clock.GetUtcNow();
        // JWT times have whole-second precision; report the same expiry the token carries.
        var expiresAt = TruncateToSeconds(now + settings.AccessTokenLifetime);

        var token = _handler.CreateToken(new SecurityTokenDescriptor
        {
            Issuer = settings.Issuer,
            Audience = settings.Audience,
            IssuedAt = now.UtcDateTime,
            NotBefore = now.UtcDateTime,
            Expires = expiresAt.UtcDateTime,
            Claims = new Dictionary<string, object>
            {
                [AccessTokenClaims.Subject] = user.Id.ToString(),
                [AccessTokenClaims.Role] = user.Role.ToClaimValue(),
                [AccessTokenClaims.SessionId] = sessionFamilyId.ToString(),
                [AccessTokenClaims.TokenId] = Guid.NewGuid().ToString(),
            },
            SigningCredentials = new SigningCredentials(CreateSigningKey(settings), SecurityAlgorithms.HmacSha256),
        });

        return new AccessToken(token, expiresAt);
    }

    /// <summary>The key used both to sign tokens here and to check them in the API's JWT bearer setup.</summary>
    public static SymmetricSecurityKey CreateSigningKey(JwtOptions settings) =>
        new(Encoding.UTF8.GetBytes(settings.SigningKey));

    private static DateTimeOffset TruncateToSeconds(DateTimeOffset value) =>
        DateTimeOffset.FromUnixTimeSeconds(value.ToUnixTimeSeconds());
}
