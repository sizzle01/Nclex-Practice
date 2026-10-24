using Nclex.Application.Auth.Sessions;

namespace Nclex.Application.Auth.Logout;

/// <summary>
/// POST /auth/logout: ends the session on this device by revoking its token family.
/// Succeeds even without a valid token, so logging out never fails.
/// </summary>
public sealed class LogoutHandler(SessionService sessions)
{
    public async Task HandleAsync(string? refreshToken, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
        {
            return;
        }

        if (await sessions.FindByTokenAsync(refreshToken, cancellationToken) is { } session)
        {
            await sessions.RevokeFamilyAsync(session.FamilyId, cancellationToken);
        }
    }
}
