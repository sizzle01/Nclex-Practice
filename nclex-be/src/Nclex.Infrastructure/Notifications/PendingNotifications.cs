using Nclex.Domain.Notifications;

namespace Nclex.Infrastructure.Notifications;

/// <summary>
/// In-app notifications added in this scope that are not committed yet. Redis must not hear
/// about a notification before Postgres has it, or a client could fetch the feed and not find it,
/// so <see cref="PublishNotificationsAfterCommit"/> sends them only once the commit succeeds.
/// </summary>
internal sealed class PendingNotifications
{
    private readonly List<Notification> _pending = [];

    public void Add(Notification notification) => _pending.Add(notification);

    /// <summary>Returns everything pending and forgets it.</summary>
    public IReadOnlyList<Notification> TakeAll()
    {
        var taken = _pending.ToList();
        _pending.Clear();
        return taken;
    }

    /// <summary>The changes were rolled back, so there is nothing to announce.</summary>
    public void Discard() => _pending.Clear();
}
