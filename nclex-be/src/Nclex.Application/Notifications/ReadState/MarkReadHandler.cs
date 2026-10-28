using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Application.Common;
using Nclex.Application.Notifications.Feed;

namespace Nclex.Application.Notifications.ReadState;

/// <summary>
/// POST /notifications/{id}/read. Marking a read notification again changes nothing. Other open
/// tabs are told, so their badges drop too.
/// </summary>
public sealed class MarkReadHandler(IAppDbContext db, ICurrentUser currentUser, INotificationBus bus, TimeProvider clock)
{
    public async Task HandleAsync(Guid notificationId, CancellationToken cancellationToken)
    {
        var notification = await db.Notifications.InAppFor(currentUser.UserId)
            .SingleOrDefaultAsync(n => n.Id == notificationId, cancellationToken)
            ?? throw new AppException(NotificationErrors.NotFound);

        if (notification.MarkRead(clock.GetUtcNow()))
        {
            await db.SaveChangesAsync(cancellationToken);
            await bus.PublishAsync(currentUser.UserId, NotificationBusMessage.ReadStateChanged, cancellationToken);
        }
    }
}
