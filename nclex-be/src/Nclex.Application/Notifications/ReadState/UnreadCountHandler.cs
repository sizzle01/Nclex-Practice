using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Application.Notifications.Feed;

namespace Nclex.Application.Notifications.ReadState;

/// <summary>The bell's badge: how many in-app notifications are unread.</summary>
public sealed record UnreadCountResponse(int Count);

/// <summary>
/// GET /notifications/unread-count. Also used by the SSE stream, which has no signed-in user,
/// so the user is passed in.
/// </summary>
public sealed class UnreadCountHandler(IAppDbContext db)
{
    public async Task<UnreadCountResponse> HandleAsync(Guid userId, CancellationToken cancellationToken) =>
        new(await db.Notifications.InAppFor(userId).CountAsync(n => n.ReadAt == null, cancellationToken));
}
