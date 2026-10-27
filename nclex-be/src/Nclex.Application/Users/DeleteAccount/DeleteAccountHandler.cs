using System.Net;
using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Application.Auth.EmailTokens;
using Nclex.Application.Auth.Lockout;
using Nclex.Application.Auth.Sessions;
using Nclex.Domain.Audit;
using Nclex.Domain.Entities;

namespace Nclex.Application.Users.DeleteAccount;

/// <summary>
/// DELETE /me: deletes the account after re-checking the password (database.md section 4).
/// Personal data is removed: sessions, emailed-link tokens and queued or sent emails are deleted,
/// the user's audit rows lose their IP addresses, and the user row is scrubbed but kept so that
/// attempts (F08 onwards) stay usable, anonymised, for item statistics.
/// </summary>
public sealed class DeleteAccountHandler(
    IAppDbContext db,
    CurrentAccount currentAccount,
    Reauthentication reauthentication,
    SessionService sessions,
    EmailTokenService emailTokens,
    IAuditLog audit,
    TimeProvider clock)
{
    public async Task HandleAsync(DeleteAccountRequest request, ClientInfo client, CancellationToken cancellationToken)
    {
        var user = await currentAccount.LoadAsync(cancellationToken);
        await reauthentication.VerifyAsync(user, request.CurrentPassword, client, cancellationToken);

        // These write straight away, outside the final save. Sessions go first: if anything later
        // fails, the user is signed out with the account intact and can simply try again.
        await sessions.DeleteAllForUserAsync(user.Id, cancellationToken);
        await emailTokens.DeleteAllForUserAsync(user.Id, cancellationToken);
        // Outbox rows hold the address and display name, even once sent.
        await db.EmailOutbox.Where(m => m.UserId == user.Id).ExecuteDeleteAsync(cancellationToken);
        await AnonymiseAuditTrailAsync(user.Id, cancellationToken);

        user.ScrubForDeletion(clock.GetUtcNow());
        // Recorded without an IP address, so the deleted account leaves no personal data behind.
        audit.RecordUserEvent(AuditActions.AccountDeleted, user.Id, ip: null);
        await db.SaveChangesAsync(cancellationToken);
    }

    // Keeps the events (what happened and when) for security investigations, without the IP
    // addresses and details that could identify the person. One UPDATE, since an account can have
    // years of login events.
    private Task<int> AnonymiseAuditTrailAsync(Guid userId, CancellationToken cancellationToken) =>
        db.AuditEvents
            .Where(a => a.ActorId == userId)
            .ExecuteUpdateAsync(
                set => set
                    .SetProperty(a => a.Ip, (IPAddress?)null)
                    .SetProperty(a => a.Metadata, AuditEvent.EmptyMetadata),
                cancellationToken);
}
