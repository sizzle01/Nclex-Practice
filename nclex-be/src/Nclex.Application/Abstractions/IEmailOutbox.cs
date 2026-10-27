using Nclex.Domain.Entities;

namespace Nclex.Application.Abstractions;

/// <summary>
/// Queues emails in the <c>email_outbox</c> table (transactional outbox, ADR-7). Nothing is sent
/// here: the row is saved by the caller's next SaveChangesAsync, in the same transaction as the
/// change that causes the email, and Nclex.Worker sends it.
/// </summary>
public interface IEmailOutbox
{
    /// <summary>Adds an email for <paramref name="recipient"/>. <paramref name="payload"/> is stored as JSON.</summary>
    /// <param name="recipient">The account the email is about.</param>
    /// <param name="templateName">A name from <see cref="Email.EmailTemplateNames"/>.</param>
    /// <param name="payload">The values the template needs, e.g. a <see cref="Email.VerifyEmailPayload"/>.</param>
    /// <param name="toAddress">
    /// Where to send it when that is not the account's current address: the new address for an
    /// email-change link, or the old one for the notice once it has changed.
    /// </param>
    void Enqueue(User recipient, string templateName, object payload, string? toAddress = null);
}
