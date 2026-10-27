using Nclex.Application.Abstractions;
using Nclex.Domain.Email;
using Nclex.Domain.Entities;
using Nclex.Infrastructure.Persistence;

namespace Nclex.Infrastructure.Email;

/// <summary>
/// Adds outbox rows to the request's DbContext, so they are saved in the same transaction as the
/// use case's other changes (ADR-7).
/// </summary>
internal sealed class EfEmailOutbox(AppDbContext db, TimeProvider clock) : IEmailOutbox
{
    public void Enqueue(User recipient, string templateName, object payload) =>
        db.EmailOutbox.Add(EmailOutboxMessage.Queue(
            recipient.Id,
            recipient.Email,
            templateName,
            EmailPayloadJson.Write(payload),
            clock.GetUtcNow()));
}
