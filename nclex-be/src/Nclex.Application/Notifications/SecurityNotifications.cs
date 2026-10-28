using Nclex.Application.Abstractions;
using Nclex.Application.Email;
using Nclex.Domain.Entities;
using Nclex.Domain.Notifications;

namespace Nclex.Application.Notifications;

/// <summary>
/// Security notices (architecture.md section 6): something changed on the account, so the owner
/// can act if it was not them. Each is an in-app <c>security</c> notification plus its email; the
/// security category is locked, so both always go out. None carries a link token.
/// Does not save; the calling use case saves the notice with the change it describes.
/// </summary>
public sealed class SecurityNotifications(INotificationPublisher notifications, TimeProvider clock)
{
    /// <summary>Where "Not you?" leads in the app.</summary>
    public const string DeepLink = "/settings/account";

    /// <summary>To the current (old) address: someone asked to move the account to <paramref name="newEmail"/>.</summary>
    public Task EmailChangeRequestedAsync(User user, string newEmail, CancellationToken cancellationToken) =>
        PublishAsync(
            user,
            NotificationCopy.EmailChangeRequested(newEmail),
            new NotificationEmail(EmailTemplateNames.EmailChangeRequested, new EmailChangeRequestedPayload(user.DisplayName, newEmail)),
            cancellationToken);

    /// <summary>To <paramref name="oldEmail"/>: the account now signs in with its new address.</summary>
    public Task EmailChangedAsync(User user, string oldEmail, CancellationToken cancellationToken) =>
        PublishAsync(
            user,
            NotificationCopy.EmailChanged(user.Email),
            new NotificationEmail(
                EmailTemplateNames.EmailChanged,
                new EmailChangedPayload(user.DisplayName, user.Email, clock.GetUtcNow()),
                ToAddress: oldEmail),
            cancellationToken);

    public Task PasswordChangedAsync(User user, CancellationToken cancellationToken) =>
        PublishAsync(
            user,
            NotificationCopy.PasswordChanged(),
            new NotificationEmail(EmailTemplateNames.PasswordChanged, new PasswordChangedPayload(user.DisplayName, clock.GetUtcNow())),
            cancellationToken);

    public Task AccountLockedAsync(User user, DateTimeOffset lockedUntil, CancellationToken cancellationToken) =>
        PublishAsync(
            user,
            NotificationCopy.AccountLocked(lockedUntil),
            new NotificationEmail(EmailTemplateNames.AccountLocked, new AccountLockedPayload(user.DisplayName, lockedUntil)),
            cancellationToken);

    public Task NewDeviceSignInAsync(User user, ClientInfo client, CancellationToken cancellationToken)
    {
        var ip = client.Ip?.ToString();
        return PublishAsync(
            user,
            NotificationCopy.NewSignIn(client.UserAgent, ip),
            new NotificationEmail(
                EmailTemplateNames.NewDeviceSignIn,
                new NewDeviceSignInPayload(user.DisplayName, clock.GetUtcNow(), client.UserAgent, ip)),
            cancellationToken);
    }

    private Task PublishAsync(User user, NotificationText text, NotificationEmail email, CancellationToken cancellationToken) =>
        notifications.PublishAsync(
            new NewNotification(
                user.Id,
                NotificationType.Security,
                NotificationCategory.Security,
                text,
                new { deepLink = DeepLink },
                Email: email),
            cancellationToken);
}
