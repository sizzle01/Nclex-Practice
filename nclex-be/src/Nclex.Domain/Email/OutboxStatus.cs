namespace Nclex.Domain.Email;

/// <summary>Delivery state of an outbox email; the <c>email_outbox.status</c> CHECK values (database.md section 2.8).</summary>
public enum OutboxStatus
{
    /// <summary>Waiting to be sent, at or after <see cref="EmailOutboxMessage.SendAfter"/>.</summary>
    Pending,

    /// <summary>Claimed by a worker that is sending it now.</summary>
    Sending,

    /// <summary>Handed to the email provider.</summary>
    Sent,

    /// <summary>Gave up after <see cref="OutboxRetryPolicy.MaxAttempts"/> failed attempts.</summary>
    Failed,
}
