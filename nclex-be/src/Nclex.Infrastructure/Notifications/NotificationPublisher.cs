using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Domain.Notifications;
using Nclex.Infrastructure.Persistence;

namespace Nclex.Infrastructure.Notifications;

/// <summary>
/// Turns a <see cref="NewNotification"/> into a <c>notifications</c> row and, if the user wants
/// email for that category, an <c>email_outbox</c> row, both in the caller's transaction
/// (transactional outbox, architecture.md 5.5). The bell hears about it after the commit.
/// </summary>
internal sealed class NotificationPublisher(
    AppDbContext db,
    IEmailOutbox outbox,
    PendingNotifications pending,
    TimeProvider clock) : INotificationPublisher
{
    private static readonly JsonSerializerOptions DataJson = new(JsonSerializerDefaults.Web);

    public async Task PublishAsync(NewNotification notification, CancellationToken cancellationToken)
    {
        if (await IsDuplicateResultAsync(notification, cancellationToken))
        {
            return;
        }

        var preference = NotificationDefaults.IsLocked(notification.Category)
            ? null
            : await db.NotificationPreferences.AsNoTracking()
                .SingleOrDefaultAsync(p => p.UserId == notification.UserId && p.Category == notification.Category, cancellationToken);
        var channels = NotificationDefaults.ChannelsFor(
            notification.Category,
            preference?.InApp,
            preference?.Email,
            hasEmail: notification.Email is not null);
        if (channels.Count == 0)
        {
            return;
        }

        var row = Notification.Create(
            notification.UserId,
            notification.Type,
            notification.Category,
            notification.Text.Title,
            notification.Text.Body,
            JsonSerializer.Serialize(notification.Data, DataJson),
            notification.RelatedAttemptId,
            channels,
            clock.GetUtcNow());
        db.Notifications.Add(row);

        if (channels.Contains(NotificationChannels.Email) && notification.Email is { } email)
        {
            // FindAsync returns the tracked user when the caller has it, e.g. right after an email change.
            var user = await db.Users.FindAsync([notification.UserId], cancellationToken)
                ?? throw new InvalidOperationException($"User {notification.UserId} does not exist.");
            outbox.Enqueue(user, email.Template, email.Payload, email.ToAddress);
        }

        if (row.IsInApp)
        {
            pending.Add(row);
        }
    }

    /// <summary>
    /// A result notification for an attempt that already has one. Grading runs with the attempt row
    /// locked, so this check cannot race; the unique index notif_result_once backs it up.
    /// </summary>
    private async Task<bool> IsDuplicateResultAsync(NewNotification notification, CancellationToken cancellationToken)
    {
        if (notification.RelatedAttemptId is not { } attemptId || !Notification.ResultTypes.Contains(notification.Type))
        {
            return false;
        }

        return db.Notifications.Local.Any(n => n.RelatedAttemptId == attemptId && Notification.ResultTypes.Contains(n.Type))
            || await db.Notifications.AnyAsync(
                n => n.RelatedAttemptId == attemptId && Notification.ResultTypes.Contains(n.Type),
                cancellationToken);
    }
}
