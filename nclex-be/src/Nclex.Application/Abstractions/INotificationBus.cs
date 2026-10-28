using Nclex.Application.Notifications;

namespace Nclex.Application.Abstractions;

/// <summary>
/// The Redis channel <c>user:{id}:notifications</c> (architecture.md 5.6). Every API replica
/// subscribes for the users whose SSE streams it holds, so any replica can deliver to any client.
/// </summary>
/// <remarks>
/// Redis only carries the signal; the notification itself is already committed in Postgres. When
/// Redis is down, publishing does nothing and clients fall back to polling the unread count.
/// </remarks>
public interface INotificationBus
{
    /// <summary>Sends <paramref name="message"/> to every open stream of <paramref name="userId"/>, now.</summary>
    Task PublishAsync(Guid userId, NotificationBusMessage message, CancellationToken cancellationToken);

    /// <summary>
    /// Calls <paramref name="onMessage"/> for each message to <paramref name="userId"/> until the
    /// returned handle is disposed.
    /// </summary>
    Task<IAsyncDisposable> SubscribeAsync(
        Guid userId,
        Func<NotificationBusMessage, Task> onMessage,
        CancellationToken cancellationToken);
}

/// <summary>What happened in a user's notifications.</summary>
/// <param name="Notification">The new notification; null when only read state changed.</param>
public sealed record NotificationBusMessage(NotificationView? Notification)
{
    /// <summary>Something was marked read, so open streams must refresh the unread count.</summary>
    public static readonly NotificationBusMessage ReadStateChanged = new((NotificationView?)null);
}
