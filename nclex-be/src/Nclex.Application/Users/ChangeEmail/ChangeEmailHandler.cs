using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Application.Auth;
using Nclex.Application.Auth.EmailTokens;
using Nclex.Application.Auth.Lockout;
using Nclex.Application.Common;
using Nclex.Application.Common.Validation;
using Nclex.Application.Notifications;
using Nclex.Domain.Audit;
using Nclex.Domain.Entities;

namespace Nclex.Application.Users.ChangeEmail;

/// <summary>
/// POST /me/email: starts an email change after re-checking the password. Nothing changes yet:
/// a confirmation link goes to the new address, and a notice to the current one (api.md section 2).
/// The change happens when the link is followed (ConfirmEmailChangeHandler).
/// </summary>
public sealed class ChangeEmailHandler(
    IAppDbContext db,
    CurrentAccount currentAccount,
    Reauthentication reauthentication,
    AccountEmails accountEmails,
    SecurityNotifications securityNotifications,
    IAuditLog audit)
{
    public async Task HandleAsync(ChangeEmailRequest request, ClientInfo client, CancellationToken cancellationToken)
    {
        var user = await currentAccount.LoadAsync(cancellationToken);
        await reauthentication.VerifyAsync(user, request.CurrentPassword, client, cancellationToken);

        if (string.Equals(request.NewEmail, user.Email, StringComparison.OrdinalIgnoreCase))
        {
            throw AppException.ForField("newEmail", ValidationErrorCodes.Unchanged);
        }

        // Checked again on confirmation, since someone may sign up with the address in between.
        // The email column is citext, so this comparison ignores case.
        if (await db.Users.AnyAsync(u => u.Email == request.NewEmail, cancellationToken))
        {
            throw new AppException(AuthErrors.EmailTaken);
        }

        await accountEmails.QueueEmailChangeConfirmationAsync(user, request.NewEmail, cancellationToken);
        await securityNotifications.EmailChangeRequestedAsync(user, request.NewEmail, cancellationToken);
        // No addresses in the audit row: they are personal data and the account keeps its history.
        audit.RecordUserEvent(AuditActions.EmailChangeRequested, user.Id, client.Ip);
        await db.SaveChangesAsync(cancellationToken);
    }
}
