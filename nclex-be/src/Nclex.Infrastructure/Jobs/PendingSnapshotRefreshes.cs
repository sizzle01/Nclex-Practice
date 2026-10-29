using Nclex.Application.Abstractions;

namespace Nclex.Infrastructure.Jobs;

/// <summary>
/// Snapshot refreshes asked for in this scope whose attempts are not committed yet.
/// <see cref="EnqueueSnapshotRefreshAfterCommit"/> queues them once the commit succeeds.
/// </summary>
internal sealed class PendingSnapshotRefreshes : ISnapshotRefreshRequests
{
    private readonly List<(Guid UserId, Guid AttemptId)> _pending = [];

    public void RequestAfterCommit(Guid userId, Guid attemptId) => _pending.Add((userId, attemptId));

    /// <summary>Returns everything pending and forgets it.</summary>
    public IReadOnlyList<(Guid UserId, Guid AttemptId)> TakeAll()
    {
        var taken = _pending.ToList();
        _pending.Clear();
        return taken;
    }

    /// <summary>The changes were rolled back, so there is nothing to refresh.</summary>
    public void Discard() => _pending.Clear();
}
