using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Nclex.Application.Abstractions;
using Nclex.Application.Notifications;

namespace Nclex.Infrastructure.Notifications;

/// <summary>
/// Publishes pending notifications to Redis right after they commit (architecture.md 5.5: COMMIT,
/// then PUBLISH). A use case with its own transaction commits in <c>CommitAsync</c>; one without
/// commits inside <c>SaveChangesAsync</c>. A rollback or a failed save drops them.
/// </summary>
/// <remarks>Only the async hooks are implemented: every use case saves asynchronously.</remarks>
internal sealed class PublishNotificationsAfterCommit(PendingNotifications pending, INotificationBus bus)
    : IDbTransactionInterceptor, ISaveChangesInterceptor
{
    public async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context?.Database.CurrentTransaction is null)
        {
            await PublishPendingAsync(cancellationToken);
        }

        return result;
    }

    public Task SaveChangesFailedAsync(DbContextErrorEventData eventData, CancellationToken cancellationToken = default)
    {
        pending.Discard();
        return Task.CompletedTask;
    }

    public Task TransactionCommittedAsync(
        DbTransaction transaction,
        TransactionEndEventData eventData,
        CancellationToken cancellationToken = default) =>
        PublishPendingAsync(cancellationToken);

    public Task TransactionRolledBackAsync(
        DbTransaction transaction,
        TransactionEndEventData eventData,
        CancellationToken cancellationToken = default)
    {
        pending.Discard();
        return Task.CompletedTask;
    }

    private async Task PublishPendingAsync(CancellationToken cancellationToken)
    {
        foreach (var notification in pending.TakeAll())
        {
            await bus.PublishAsync(notification.UserId, new NotificationBusMessage(NotificationView.From(notification)), cancellationToken);
        }
    }
}
