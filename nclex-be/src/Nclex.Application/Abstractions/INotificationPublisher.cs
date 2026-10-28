using Nclex.Domain.Notifications;

namespace Nclex.Application.Abstractions;

/// <summary>
/// Creates notifications and, per the user's preferences, their emails (interfaces.md section 3,
/// architecture.md 5.6). Producers build a <see cref="NewNotification"/> with the factories in
/// <c>Nclex.Application.Notifications</c>.
/// </summary>
public interface INotificationPublisher
{
    /// <summary>
    /// Adds the notification and its email_outbox row to the current DbContext, so they commit in the
    /// caller's transaction. Publishes to Redis for the bell only after that commit.
    /// </summary>
    /// <remarks>
    /// A result notification for an attempt that already has one is skipped, so a retried grading
    /// never notifies twice (index <c>notif_result_once</c> is the backstop).
    /// </remarks>
    Task PublishAsync(NewNotification notification, CancellationToken cancellationToken);
}

/// <summary>A notification to create, before preferences decide its channels.</summary>
/// <param name="UserId">Who it is for.</param>
/// <param name="Type">Which template it follows.</param>
/// <param name="Category">The preference group that decides its channels.</param>
/// <param name="Text">Title and body for the bell and the feed.</param>
/// <param name="Data">Values for the client, serialised as camelCase JSON, e.g. <c>{ deepLink }</c>.</param>
/// <param name="RelatedAttemptId">The attempt it is about, if any.</param>
/// <param name="Email">The email to send if the user wants this category by email; null for in-app only.</param>
public sealed record NewNotification(
    Guid UserId,
    NotificationType Type,
    NotificationCategory Category,
    NotificationText Text,
    object Data,
    Guid? RelatedAttemptId = null,
    NotificationEmail? Email = null);

/// <summary>The email that goes with a notification.</summary>
/// <param name="Template">A name from <see cref="Email.EmailTemplateNames"/>.</param>
/// <param name="Payload">The values the template needs.</param>
/// <param name="ToAddress">Where to send it when not the account's current address (see <see cref="IEmailOutbox"/>).</param>
public sealed record NotificationEmail(string Template, object Payload, string? ToAddress = null);
