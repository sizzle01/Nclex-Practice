using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Domain.Entities;

namespace Nclex.Application.Auth.Sessions;

/// <summary>
/// Creates, rotates and revokes refresh sessions (architecture.md section 6, "Sessions").
/// Methods that add rows do not save; the calling use case saves once, in one transaction.
/// </summary>
public sealed class SessionService(
    IAppDbContext db,
    ISecureTokenGenerator tokens,
    IAccessTokenIssuer accessTokens,
    TimeProvider clock)
{
    /// <summary>Starts a new session family for a user who has just signed up or logged in.</summary>
    public IssuedSession Start(User user, ClientInfo client)
    {
        var refreshToken = tokens.Generate();
        var session = RefreshSession.Start(user.Id, refreshToken.Hash, client, clock.GetUtcNow());
        db.RefreshSessions.Add(session);
        return Issue(user, refreshToken, session);
    }

    /// <summary>Revokes <paramref name="session"/> and adds its successor in the same family.</summary>
    public IssuedSession Rotate(RefreshSession session, User user, ClientInfo client)
    {
        var refreshToken = tokens.Generate();
        var successor = session.Rotate(refreshToken.Hash, client, clock.GetUtcNow());
        db.RefreshSessions.Add(successor);
        return Issue(user, refreshToken, successor);
    }

    /// <summary>Finds the session for a refresh token from a cookie, with its user. Null if unknown.</summary>
    public Task<RefreshSession?> FindByTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var tokenHash = tokens.Hash(refreshToken);
        return db.RefreshSessions
            .Include(s => s.User)
            .SingleOrDefaultAsync(s => s.TokenHash == tokenHash, cancellationToken);
    }

    /// <summary>Revokes every still-active token in a family. Writes immediately.</summary>
    public Task RevokeFamilyAsync(Guid familyId, CancellationToken cancellationToken)
    {
        DateTimeOffset? now = clock.GetUtcNow();
        return db.RefreshSessions
            .Where(s => s.FamilyId == familyId && s.RevokedAt == null)
            .ExecuteUpdateAsync(set => set.SetProperty(s => s.RevokedAt, now), cancellationToken);
    }

    /// <summary>Revokes every active session of a user ("log out everywhere"). Writes immediately.</summary>
    public Task RevokeAllForUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        DateTimeOffset? now = clock.GetUtcNow();
        return db.RefreshSessions
            .Where(s => s.UserId == userId && s.RevokedAt == null)
            .ExecuteUpdateAsync(set => set.SetProperty(s => s.RevokedAt, now), cancellationToken);
    }

    private IssuedSession Issue(User user, SecureToken refreshToken, RefreshSession session) =>
        new(accessTokens.Issue(user), new RefreshToken(refreshToken.Value, session.ExpiresAt));
}
