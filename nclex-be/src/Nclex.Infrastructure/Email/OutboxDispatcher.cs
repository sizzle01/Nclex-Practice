using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nclex.Application.Abstractions;
using Nclex.Domain.Email;
using Nclex.Infrastructure.Email.Templates;
using Nclex.Infrastructure.Persistence;

namespace Nclex.Infrastructure.Email;

/// <summary>
/// Sends due emails from the outbox (architecture.md section 5.5). Nclex.Worker calls it every few
/// seconds. Several workers can run at once: each claims different rows, so no email is sent twice
/// by two workers.
/// </summary>
public sealed partial class OutboxDispatcher(
    AppDbContext db,
    EmailTemplates templates,
    IEmailSender sender,
    TimeProvider clock,
    ILogger<OutboxDispatcher> logger)
{
    /// <summary>Most emails claimed in one go, so one worker never holds a large share of the queue.</summary>
    public const int BatchSize = 20;

    /// <summary>Claims up to <see cref="BatchSize"/> due emails and tries to send each one.</summary>
    /// <returns>How many emails were attempted.</returns>
    public async Task<int> DispatchDueAsync(CancellationToken cancellationToken)
    {
        var claimed = await ClaimDueAsync(cancellationToken);
        foreach (var message in claimed)
        {
            await SendAsync(message, cancellationToken);
        }

        return claimed.Count;
    }

    /// <summary>
    /// Returns emails to pending whose worker stopped mid-send (crash, deploy), once their claim
    /// lease has run out. Called less often than <see cref="DispatchDueAsync"/>: it cannot use the
    /// pending-only index, and stuck claims are rare.
    /// </summary>
    /// <returns>How many emails were released.</returns>
    public async Task<int> ReleaseExpiredClaimsAsync(CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        var stuck = await db.EmailOutbox
            .Where(m => m.Status == OutboxStatus.Sending && m.SendAfter <= now)
            .ToListAsync(cancellationToken);
        var released = stuck.Count(m => m.ReleaseExpiredClaim(now));
        await db.SaveChangesAsync(cancellationToken);

        if (released > 0)
        {
            LogClaimsReleased(logger, released);
        }

        return released;
    }

    // Marks due rows as "sending" in one short transaction, before any email is sent, so a slow
    // SMTP server never holds row locks. FOR UPDATE SKIP LOCKED makes concurrent workers skip rows
    // another worker is claiming instead of waiting for them or claiming them too.
    // The WHERE clause is EmailOutboxMessage.IsDueAt, written so it can use outbox_pending_idx.
    private async Task<List<EmailOutboxMessage>> ClaimDueAsync(CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        var due = await db.EmailOutbox
            .FromSql($"""
                SELECT * FROM email_outbox
                WHERE status = 'pending' AND send_after <= {now}
                ORDER BY send_after
                LIMIT {BatchSize}
                FOR UPDATE SKIP LOCKED
                """)
            .ToListAsync(cancellationToken);
        foreach (var message in due)
        {
            message.StartAttempt(now);
        }

        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return due;
    }

    private async Task SendAsync(EmailOutboxMessage message, CancellationToken cancellationToken)
    {
        try
        {
            var template = templates.Get(message.Template);
            var content = template.Render(message.Payload);
            var providerId = await sender.SendAsync(
                new EmailMessage(message.ToEmail, content.Subject, content.HtmlBody, content.TextBody),
                cancellationToken);
            message.MarkSent(providerId, redactPayload: template.PayloadHoldsSecret, clock.GetUtcNow());
            LogSent(logger, message.Id, message.Template, message.Attempts);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            message.MarkAttemptFailed($"{ex.GetType().Name}: {ex.Message}", clock.GetUtcNow());
            LogAttemptFailed(logger, ex, message.Id, message.Template, message.Attempts, message.Status);
        }

        // Saved after every email, and even during shutdown, so an email that went out is never
        // left "sending" and sent again after its lease runs out.
        await db.SaveChangesAsync(CancellationToken.None);
    }

    // Log ids and template names only: payloads hold link tokens and addresses are personal data.
    [LoggerMessage(Level = LogLevel.Information, Message = "Sent outbox email {OutboxId} ({Template}) on attempt {Attempt}")]
    private static partial void LogSent(ILogger logger, Guid outboxId, string template, int attempt);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Outbox email {OutboxId} ({Template}) failed on attempt {Attempt}; now {Status}")]
    private static partial void LogAttemptFailed(ILogger logger, Exception exception, Guid outboxId, string template, int attempt, OutboxStatus status);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Released {Count} outbox emails whose worker stopped mid-send")]
    private static partial void LogClaimsReleased(ILogger logger, int count);
}
