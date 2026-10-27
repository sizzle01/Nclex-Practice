using Nclex.Application.Abstractions;
using Nclex.Domain.Entities;

namespace Nclex.Application.Email;

/// <summary>
/// Queues the security notices (architecture.md section 6): emails that tell the owner something
/// changed on their account, so they can act if it was not them. None carries a link token.
/// Does not save; the calling use case saves the notice with the change it describes.
/// </summary>
public sealed class SecurityEmails(IEmailOutbox outbox, TimeProvider clock)
{
    /// <summary>To the current (old) address: someone asked to move the account to <paramref name="newEmail"/>.</summary>
    public void QueueEmailChangeRequested(User user, string newEmail) =>
        outbox.Enqueue(user, EmailTemplateNames.EmailChangeRequested, new EmailChangeRequestedPayload(user.DisplayName, newEmail));

    /// <summary>To <paramref name="oldEmail"/>: the account now signs in with its new address.</summary>
    public void QueueEmailChanged(User user, string oldEmail) =>
        outbox.Enqueue(
            user,
            EmailTemplateNames.EmailChanged,
            new EmailChangedPayload(user.DisplayName, user.Email, clock.GetUtcNow()),
            toAddress: oldEmail);

    public void QueuePasswordChanged(User user) =>
        outbox.Enqueue(user, EmailTemplateNames.PasswordChanged, new PasswordChangedPayload(user.DisplayName, clock.GetUtcNow()));

    public void QueueAccountLocked(User user, DateTimeOffset lockedUntil) =>
        outbox.Enqueue(user, EmailTemplateNames.AccountLocked, new AccountLockedPayload(user.DisplayName, lockedUntil));

    public void QueueNewDeviceSignIn(User user, ClientInfo client) =>
        outbox.Enqueue(
            user,
            EmailTemplateNames.NewDeviceSignIn,
            new NewDeviceSignInPayload(user.DisplayName, clock.GetUtcNow(), client.UserAgent, client.Ip?.ToString()));
}
