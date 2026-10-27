namespace Nclex.Domain.Email;

/// <summary>
/// An email waiting to be sent, or already sent. Maps to the <c>email_outbox</c> table
/// (database.md section 2.8).
/// </summary>
/// <remarks>
/// Transactional outbox (ADR-7, architecture.md section 5.5): a use case adds the row in the same
/// transaction as the change that causes the email, and Nclex.Worker sends it later. A crash can
/// then never lose an email or send one for a change that was rolled back.
/// </remarks>
public sealed class EmailOutboxMessage
{
    /// <summary>Longest error text kept in <see cref="LastError"/>.</summary>
    public const int LastErrorMaxLength = 1000;

    /// <summary>
    /// How long a worker may hold a claimed email. If the worker crashes mid-send, the row is
    /// released back to pending after this (see <see cref="ReleaseExpiredClaim"/>), so another
    /// worker retries it instead of it staying "sending" forever.
    /// </summary>
    public static readonly TimeSpan ClaimLease = TimeSpan.FromMinutes(10);

    /// <summary>What <see cref="Payload"/> becomes after sending when it held a secret.</summary>
    public const string RedactedPayload = "{}";

    // For EF Core, which sets properties directly when loading rows.
    private EmailOutboxMessage()
    {
    }

    public Guid Id { get; private set; }

    /// <summary>The recipient's account, if they have one.</summary>
    public Guid? UserId { get; private set; }

    public string ToEmail { get; private set; } = string.Empty;

    /// <summary>Template name, e.g. "verify_email"; the worker renders it with <see cref="Payload"/>.</summary>
    public string Template { get; private set; } = string.Empty;

    /// <summary>JSON values the template needs (stored as jsonb).</summary>
    public string Payload { get; private set; } = RedactedPayload;

    public OutboxStatus Status { get; private set; }

    /// <summary>Send attempts made so far, counted when a worker claims the row (<see cref="StartAttempt"/>).</summary>
    public short Attempts { get; private set; }

    /// <summary>Earliest time the next attempt may start.</summary>
    public DateTimeOffset SendAfter { get; private set; }

    public DateTimeOffset? SentAt { get; private set; }

    /// <summary>The provider's id for the sent message, for tracing deliveries.</summary>
    public string? ProviderId { get; private set; }

    public string? LastError { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>Queues an email to be sent as soon as a worker picks it up.</summary>
    public static EmailOutboxMessage Queue(Guid? userId, string toEmail, string template, string payload, DateTimeOffset now) =>
        new()
        {
            Id = Guid.CreateVersion7(now),
            UserId = userId,
            ToEmail = toEmail,
            Template = template,
            Payload = payload,
            Status = OutboxStatus.Pending,
            SendAfter = now,
            CreatedAt = now,
        };

    /// <summary>True if a worker may claim this email at <paramref name="now"/>.</summary>
    public bool IsDueAt(DateTimeOffset now) => Status == OutboxStatus.Pending && SendAfter <= now;

    /// <summary>
    /// Claims the email for one send attempt. While claimed, <see cref="SendAfter"/> holds the end of
    /// the claim lease.
    /// </summary>
    public void StartAttempt(DateTimeOffset now)
    {
        if (!IsDueAt(now))
        {
            throw new InvalidOperationException("Only a due email can be claimed.");
        }

        Status = OutboxStatus.Sending;
        Attempts++;
        SendAfter = now + ClaimLease;
    }

    /// <summary>
    /// Returns an email to pending if a worker claimed it and its lease ran out, which means that
    /// worker stopped before recording the result. The attempt still counts towards the limit.
    /// </summary>
    /// <returns>True if the claim was released.</returns>
    public bool ReleaseExpiredClaim(DateTimeOffset now)
    {
        if (Status != OutboxStatus.Sending || SendAfter > now)
        {
            return false;
        }

        Status = OutboxStatus.Pending;
        return true;
    }

    /// <summary>
    /// Records a successful send. A payload that holds a secret (such as a link token) is cleared,
    /// so the database does not keep usable links once they have been delivered.
    /// </summary>
    public void MarkSent(string providerId, bool redactPayload, DateTimeOffset now)
    {
        if (Status != OutboxStatus.Sending)
        {
            throw new InvalidOperationException("Only an email being sent can be marked sent.");
        }

        Status = OutboxStatus.Sent;
        SentAt = now;
        ProviderId = providerId;
        LastError = null;
        if (redactPayload)
        {
            Payload = RedactedPayload;
        }
    }

    /// <summary>Records a failed attempt and schedules a retry, or gives up after the last attempt.</summary>
    public void MarkAttemptFailed(string error, DateTimeOffset now)
    {
        if (Status != OutboxStatus.Sending)
        {
            throw new InvalidOperationException("Only an email being sent can fail.");
        }

        LastError = error.Length > LastErrorMaxLength ? error[..LastErrorMaxLength] : error;

        if (OutboxRetryPolicy.NextAttemptAt(Attempts, now) is { } retryAt)
        {
            Status = OutboxStatus.Pending;
            SendAfter = retryAt;
        }
        else
        {
            Status = OutboxStatus.Failed;
        }
    }
}
