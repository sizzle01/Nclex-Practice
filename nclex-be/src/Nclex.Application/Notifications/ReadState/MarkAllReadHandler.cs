using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Application.Notifications.Feed;

namespace Nclex.Application.Notifications.ReadState;

/// <summary>POST /notifications/read-all: one UPDATE, however many are unread.</summary>
public sealed class MarkAllReadHandler(IAppDbContext db, ICurrentUser currentUser, INotificationBus bus, TimeProvider clock)
{
    public async Task HandleAsync(CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        var marked = await db.Notifications.InAppFor(currentUser.UserId)
            .Where(n => n.ReadAt == null)
            .ExecuteUpdateAsync(n => n.SetProperty(x => x.ReadAt, now), cancellationToken);

        if (marked > 0)
        {
            await bus.PublishAsync(currentUser.UserId, NotificationBusMessage.ReadStateChanged, cancellationToken);
        }
    }
}
