using Nclex.Application.Abstractions;
using Nclex.Application.Auth.Lockout;
using Nclex.Application.Auth.Sessions;
using Nclex.Application.Email;
using Nclex.Domain.Audit;
using Nclex.Domain.Entities;

namespace Nclex.Application.Users.ChangePassword;

/// <summary>
/// POST /me/password: sets a new password after re-checking the current one. This device stays
/// signed in; every other session is revoked, because a password change is often a reaction to
/// someone else having it. The owner gets a "password changed" email.
/// </summary>
public sealed class ChangePasswordHandler(
    IAppDbContext db,
    CurrentAccount currentAccount,
    Reauthentication reauthentication,
    IPasswordHasher passwordHasher,
    SessionService sessions,
    SecurityEmails securityEmails,
    IAuditLog audit,
    ICurrentUser currentUser,
    TimeProvider clock)
{
    public async Task HandleAsync(ChangePasswordRequest request, ClientInfo client, CancellationToken cancellationToken)
    {
        var user = await currentAccount.LoadAsync(cancellationToken);
        await reauthentication.VerifyAsync(user, request.CurrentPassword, client, cancellationToken);

        // Revoke first, as in ResetPasswordHandler: if the save below fails, the worst case is
        // other devices signed out with the old password still working.
        await sessions.RevokeOtherSessionsAsync(user.Id, keepFamilyId: currentUser.SessionFamilyId, cancellationToken);

        user.ChangePassword(passwordHasher.Hash(request.NewPassword), clock.GetUtcNow());
        securityEmails.QueuePasswordChanged(user);
        audit.RecordUserEvent(AuditActions.PasswordChanged, user.Id, client.Ip);
        await db.SaveChangesAsync(cancellationToken);
    }
}
