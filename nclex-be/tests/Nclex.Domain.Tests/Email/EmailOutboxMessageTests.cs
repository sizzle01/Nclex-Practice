using Nclex.Domain.Email;

namespace Nclex.Domain.Tests.Email;

public sealed class EmailOutboxMessageTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 27, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void A_queued_message_is_pending_and_due_immediately()
    {
        var message = NewMessage();

        message.Status.ShouldBe(OutboxStatus.Pending);
        message.Attempts.ShouldBe((short)0);
        message.IsDueAt(Now).ShouldBeTrue();
    }

    [Fact]
    public void Starting_an_attempt_counts_it_and_holds_a_lease()
    {
        var message = NewMessage();

        message.StartAttempt(Now);

        message.Status.ShouldBe(OutboxStatus.Sending);
        message.Attempts.ShouldBe((short)1);
        message.IsDueAt(Now.AddMinutes(9)).ShouldBeFalse();
    }

    [Fact]
    public void A_claim_is_released_only_once_its_lease_runs_out()
    {
        var message = NewMessage();
        message.StartAttempt(Now);

        message.ReleaseExpiredClaim(Now + EmailOutboxMessage.ClaimLease - TimeSpan.FromSeconds(1)).ShouldBeFalse();
        message.ReleaseExpiredClaim(Now + EmailOutboxMessage.ClaimLease).ShouldBeTrue();

        message.Status.ShouldBe(OutboxStatus.Pending);
        message.Attempts.ShouldBe((short)1);
        message.IsDueAt(Now + EmailOutboxMessage.ClaimLease).ShouldBeTrue();
    }

    [Fact]
    public void Releasing_does_not_touch_pending_or_sent_messages()
    {
        var pending = NewMessage();
        var sent = NewMessage();
        sent.StartAttempt(Now);
        sent.MarkSent("id", redactPayload: false, Now);

        pending.ReleaseExpiredClaim(Now.AddDays(1)).ShouldBeFalse();
        sent.ReleaseExpiredClaim(Now.AddDays(1)).ShouldBeFalse();
        sent.Status.ShouldBe(OutboxStatus.Sent);
    }

    [Fact]
    public void Sending_a_message_that_held_a_secret_clears_its_payload()
    {
        var message = NewMessage();
        message.StartAttempt(Now);

        message.MarkSent("provider-1", redactPayload: true, Now.AddSeconds(1));

        message.Status.ShouldBe(OutboxStatus.Sent);
        message.SentAt.ShouldBe(Now.AddSeconds(1));
        message.ProviderId.ShouldBe("provider-1");
        message.Payload.ShouldBe(EmailOutboxMessage.RedactedPayload);
        message.IsDueAt(Now.AddDays(1)).ShouldBeFalse();
    }

    [Fact]
    public void Sending_a_message_without_secrets_keeps_its_payload()
    {
        var message = NewMessage();
        message.StartAttempt(Now);

        message.MarkSent("provider-1", redactPayload: false, Now);

        message.Payload.ShouldBe("""{"token":"abc"}""");
    }

    [Fact]
    public void A_failed_attempt_schedules_a_retry()
    {
        var message = NewMessage();
        message.StartAttempt(Now);

        message.MarkAttemptFailed("SMTP timeout", Now);

        message.Status.ShouldBe(OutboxStatus.Pending);
        message.SendAfter.ShouldBe(Now.AddSeconds(30));
        message.LastError.ShouldBe("SMTP timeout");
    }

    [Fact]
    public void The_last_failed_attempt_marks_the_message_failed()
    {
        var message = NewMessage();
        var now = Now;
        for (var attempt = 1; attempt <= OutboxRetryPolicy.MaxAttempts; attempt++)
        {
            message.StartAttempt(now);
            message.MarkAttemptFailed("down", now);
            now = message.SendAfter;
        }

        message.Status.ShouldBe(OutboxStatus.Failed);
        message.Attempts.ShouldBe((short)OutboxRetryPolicy.MaxAttempts);
        message.IsDueAt(now.AddDays(1)).ShouldBeFalse();
    }

    [Fact]
    public void Long_errors_are_truncated()
    {
        var message = NewMessage();
        message.StartAttempt(Now);

        message.MarkAttemptFailed(new string('x', 5000), Now);

        message.LastError!.Length.ShouldBe(EmailOutboxMessage.LastErrorMaxLength);
    }

    [Fact]
    public void A_message_that_is_not_being_sent_cannot_be_marked_sent()
    {
        Should.Throw<InvalidOperationException>(() => NewMessage().MarkSent("id", redactPayload: false, Now));
    }

    private static EmailOutboxMessage NewMessage() =>
        EmailOutboxMessage.Queue(Guid.NewGuid(), "ada@example.com", "verify_email", """{"token":"abc"}""", Now);
}
