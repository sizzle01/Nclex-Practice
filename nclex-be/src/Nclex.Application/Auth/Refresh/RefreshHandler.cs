using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nclex.Application.Abstractions;
using Nclex.Application.Auth.Sessions;
using Nclex.Application.Common;
using Nclex.Domain.Entities;

namespace Nclex.Application.Auth.Refresh;

/// <summary>
/// POST /auth/refresh: swaps a refresh token for a new access token and a new refresh token
/// (rotation). Presenting a token that was already rotated revokes its whole family
/// (reuse detection, architecture.md section 6).
/// </summary>
public sealed partial class RefreshHandler(
    IAppDbContext db,
    SessionService sessions,
    TimeProvider clock,
    ILogger<RefreshHandler> logger)
{
    public async Task<IssuedSession> HandleAsync(string? refreshToken, ClientInfo client, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
        {
            throw new AppException(CommonErrors.Unauthenticated);
        }

        var session = await sessions.FindByTokenAsync(refreshToken, cancellationToken)
            ?? throw new AppException(CommonErrors.Unauthenticated);

        switch (session.StateAt(clock.GetUtcNow()))
        {
            case RefreshSessionState.Revoked:
                await RevokeFamilyAfterReuseAsync(session, cancellationToken);
                throw new AppException(CommonErrors.Unauthenticated);
            case RefreshSessionState.Expired:
                throw new AppException(CommonErrors.Unauthenticated);
        }

        if (session.User is not { IsDeleted: false } user)
        {
            throw new AppException(CommonErrors.Unauthenticated);
        }

        var issued = sessions.Rotate(session, user, client);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Another request rotated this token between our read and our write: the same token
            // was used twice, which is exactly what reuse detection guards against.
            await RevokeFamilyAfterReuseAsync(session, cancellationToken);
            throw new AppException(CommonErrors.Unauthenticated);
        }

        return issued;
    }

    private async Task RevokeFamilyAfterReuseAsync(RefreshSession session, CancellationToken cancellationToken)
    {
        LogRefreshTokenReuse(logger, session.UserId, session.FamilyId);
        await sessions.RevokeFamilyAsync(session.FamilyId, cancellationToken);
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Refresh token reuse detected for user {UserId}; revoking session family {FamilyId}")]
    private static partial void LogRefreshTokenReuse(ILogger logger, Guid userId, Guid familyId);
}
