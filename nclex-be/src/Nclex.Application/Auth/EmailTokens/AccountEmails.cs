using Nclex.Application.Abstractions;
using Nclex.Application.Email;
using Nclex.Domain.Entities;

namespace Nclex.Application.Auth.EmailTokens;

/// <summary>
/// Queues the account emails that carry a link: a new token plus an outbox row. Does not save;
/// the calling use case saves both rows together with its own changes.
/// </summary>
public sealed class AccountEmails(EmailTokenService emailTokens, IEmailOutbox outbox)
{
    /// <summary>Queues a "verify your email" email with a fresh 24-hour link.</summary>
    public async Task QueueVerificationAsync(User user, CancellationToken cancellationToken)
    {
        var token = await emailTokens.IssueAsync(user, EmailTokenPurpose.VerifyEmail, cancellationToken);
        outbox.Enqueue(user, EmailTemplateNames.VerifyEmail, new VerifyEmailPayload(user.DisplayName, token));
    }

    /// <summary>Queues a "reset your password" email with a fresh 30-minute link.</summary>
    public async Task QueuePasswordResetAsync(User user, CancellationToken cancellationToken)
    {
        var token = await emailTokens.IssueAsync(user, EmailTokenPurpose.ResetPassword, cancellationToken);
        outbox.Enqueue(user, EmailTemplateNames.ResetPassword, new ResetPasswordPayload(user.DisplayName, token));
    }
}
