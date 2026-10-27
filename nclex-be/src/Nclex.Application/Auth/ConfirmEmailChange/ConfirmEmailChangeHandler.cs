using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Application.Auth.EmailTokens;
using Nclex.Application.Common;
using Nclex.Application.Email;
using Nclex.Domain.Audit;
using Nclex.Domain.Entities;

namespace Nclex.Application.Auth.ConfirmEmailChange;

/// <summary>
/// POST /auth/email/confirm-change: uses the link sent to the new address to switch the account
/// to it. Like the other emailed links, it needs no sign-in, so it works on any device. The old
/// address gets a notice that the change went through.
/// </summary>
public sealed class ConfirmEmailChangeHandler(
    IAppDbContext db,
    EmailTokenService emailTokens,
    SecurityEmails securityEmails,
    IAuditLog audit,
    TimeProvider clock)
{
    public async Task HandleAsync(ConfirmEmailChangeRequest request, ClientInfo client, CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        var emailToken = await emailTokens.FindUsableAsync(request.Token, EmailTokenPurpose.ChangeEmail, now, cancellationToken)
            ?? throw new AppException(AuthErrors.InvalidToken);
        var user = emailToken.User!; // FindUsableAsync loads the user
        var newEmail = emailToken.NewEmail!; // always set for change-email tokens

        // Someone may have signed up with this address since the link was sent.
        if (await db.Users.AnyAsync(u => u.Email == newEmail && u.Id != user.Id, cancellationToken))
        {
            throw new AppException(AuthErrors.EmailTaken);
        }

        var oldEmail = user.Email;
        emailToken.Consume(now);
        user.ChangeEmail(newEmail, now);
        securityEmails.QueueEmailChanged(user, oldEmail);
        audit.RecordUserEvent(AuditActions.EmailChanged, user.Id, client.Ip);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (UniqueConstraintViolationException)
        {
            // A sign-up with the same address won the race between the check above and this save.
            throw new AppException(AuthErrors.EmailTaken);
        }
        catch (DbUpdateConcurrencyException)
        {
            // The same link was used by another request between our read and our write.
            throw new AppException(AuthErrors.InvalidToken);
        }
    }
}
