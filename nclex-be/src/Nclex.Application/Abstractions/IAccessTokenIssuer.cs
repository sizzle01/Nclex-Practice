using Nclex.Domain.Entities;

namespace Nclex.Application.Abstractions;

/// <summary>Issues short-lived access tokens (JWT, 15 minutes, architecture.md section 6).</summary>
public interface IAccessTokenIssuer
{
    /// <summary>Issues a token for <paramref name="user"/>, tied to the sign-in <paramref name="sessionFamilyId"/>.</summary>
    AccessToken Issue(User user, Guid sessionFamilyId);
}

/// <summary>A signed access token and the moment it stops being accepted.</summary>
public sealed record AccessToken(string Value, DateTimeOffset ExpiresAt);
