using Nclex.Domain.Entities;

namespace Nclex.Application.Abstractions;

/// <summary>Issues short-lived access tokens (JWT, 15 minutes, architecture.md section 6).</summary>
public interface IAccessTokenIssuer
{
    AccessToken Issue(User user);
}

/// <summary>A signed access token and the moment it stops being accepted.</summary>
public sealed record AccessToken(string Value, DateTimeOffset ExpiresAt);
