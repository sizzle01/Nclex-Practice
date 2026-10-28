using System.Text.Json;
using Nclex.Domain.Notifications;

namespace Nclex.Application.Notifications;

/// <summary>The notification object of api.md section 6, as the feed and the SSE stream send it.</summary>
/// <param name="Data">Free-form values such as <c>deepLink</c>; differs per type.</param>
public sealed record NotificationView(
    Guid Id,
    NotificationType Type,
    NotificationCategory Category,
    string Title,
    string Body,
    JsonElement Data,
    Guid? RelatedAttemptId,
    DateTimeOffset? ReadAt,
    DateTimeOffset CreatedAt)
{
    public static NotificationView From(Notification notification)
    {
        using var data = JsonDocument.Parse(notification.Data);
        return new NotificationView(
            notification.Id,
            notification.Type,
            notification.Category,
            notification.Title,
            notification.Body,
            data.RootElement.Clone(),
            notification.RelatedAttemptId,
            notification.ReadAt,
            notification.CreatedAt);
    }
}
