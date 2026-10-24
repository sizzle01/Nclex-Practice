using Nclex.Application.Abstractions;
using Nclex.Application.Auth.Sessions;

namespace Nclex.Application.Auth.LogoutAll;

/// <summary>
/// POST /auth/logout-all: revokes every refresh session of the signed-in user, on every device.
/// Access tokens already issued stay valid until they expire (at most 15 minutes).
/// </summary>
public sealed class LogoutAllHandler(SessionService sessions, ICurrentUser currentUser)
{
    public Task HandleAsync(CancellationToken cancellationToken) =>
        sessions.RevokeAllForUserAsync(currentUser.UserId, cancellationToken);
}
