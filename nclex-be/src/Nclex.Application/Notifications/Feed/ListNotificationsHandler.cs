using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Domain.Notifications;

namespace Nclex.Application.Notifications.Feed;

/// <summary>GET /notifications: the signed-in user's in-app notifications, newest first.</summary>
public sealed class ListNotificationsHandler(IAppDbContext db, ICurrentUser currentUser)
{
    public async Task<NotificationPage> HandleAsync(ListNotificationsRequest request, CancellationToken cancellationToken)
    {
        var limit = request.Limit ?? ListNotificationsRequest.DefaultLimit;
        var query = db.Notifications.AsNoTracking().InAppFor(currentUser.UserId);
        if (request.UnreadOnly)
        {
            query = query.Where(n => n.ReadAt == null);
        }

        if (request.Cursor is { } cursor && FeedCursor.TryDecode(cursor, out var createdAt, out var id))
        {
            query = query.Where(n => n.CreatedAt < createdAt || (n.CreatedAt == createdAt && n.Id < id));
        }

        // One extra row tells whether another page follows.
        var rows = await query
            .OrderByDescending(n => n.CreatedAt)
            .ThenByDescending(n => n.Id)
            .Take(limit + 1)
            .ToListAsync(cancellationToken);

        var page = rows.Take(limit).ToList();
        var next = rows.Count > limit ? FeedCursor.Encode(page[^1].CreatedAt, page[^1].Id) : null;
        return new NotificationPage([.. page.Select(NotificationView.From)], next);
    }
}

/// <summary>Filters shared by the feed, the unread count and the read endpoints.</summary>
public static class NotificationQueries
{
    /// <summary>
    /// A user's notifications that were delivered in-app. Rows sent only by email (the user turned
    /// in-app off for that category) are kept for exactly-once checks but never shown.
    /// </summary>
    public static IQueryable<Notification> InAppFor(this IQueryable<Notification> notifications, Guid userId) =>
        notifications.Where(n => n.UserId == userId && n.Channels.Contains(NotificationChannels.InApp));
}
