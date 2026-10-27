using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Application.Auth.EmailTokens;
using Nclex.Application.Auth.Sessions;
using Nclex.Application.Common;
using Nclex.Application.Email;
using Nclex.Domain.Audit;
using Nclex.Domain.Entities;

namespace Nclex.Application.Auth.ResetPassword;

/// <summary>
/// POST /auth/password/reset: uses a reset link's token to set a new password, and optionally
/// signs the user out on every device (architecture.md section 6, "Email tokens").
/// The user is not signed in here; the frontend sends them to the login page afterwards.
/// The owner gets a "password changed" email (api.md section 2).
/// </summary>
public sealed class ResetPasswordHandler(
    IAppDbContext db,
    EmailTokenService emailTokens,
    IPasswordHasher passwordHasher,
    SessionService sessions,
    SecurityEmails securityEmails,
    IAuditLog audit,
    TimeProvider clock)
{
    public async Task HandleAsync(ResetPasswordRequest request, ClientInfo client, CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        var emailToken = await emailTokens.FindUsableAsync(request.Token, EmailTokenPurpose.ResetPassword, now, cancellationToken)
            ?? throw new AppException(AuthErrors.InvalidToken);
        var user = emailToken.User!; // FindUsableAsync loads the user

        // Revoke first: if the save below then fails, the worst case is being signed out with the
        // old password still working, never a new password with an attacker's session still alive.
        if (request.SignOutEverywhere)
        {
            await sessions.RevokeAllForUserAsync(user.Id, cancellationToken);
        }

        emailToken.Consume(now);
        user.ChangePassword(passwordHasher.Hash(request.NewPassword), now);
        // Following a link sent to the address proves the user owns it.
        user.MarkEmailVerified(now);
        securityEmails.QueuePasswordChanged(user);
        audit.RecordUserEvent(AuditActions.PasswordReset, user.Id, client.Ip, new { request.SignOutEverywhere });

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            // The same link was used by another request between our read and our write.
            throw new AppException(AuthErrors.InvalidToken);
        }
    }
}
