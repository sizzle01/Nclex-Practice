using Nclex.Application.Abstractions;

namespace Nclex.Infrastructure.Notifications;

/// <summary>
/// Stands in for the notification pipeline until F12 builds the notifications table, the email
/// templates and the Redis fan-out (architecture.md 5.6). Grading already calls it inside the
/// submit transaction, so F12 only replaces this class.
/// </summary>
internal sealed class NoOpNotificationPublisher : INotificationPublisher
{
    public Task PublishAsync(NewNotification notification, CancellationToken cancellationToken) => Task.CompletedTask;
}
