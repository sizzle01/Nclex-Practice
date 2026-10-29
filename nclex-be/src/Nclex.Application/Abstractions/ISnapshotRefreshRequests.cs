namespace Nclex.Application.Abstractions;

/// <summary>
/// Asks for a performance snapshot refresh once the current transaction commits (architecture.md
/// 5.5: COMMIT, then enqueue SnapshotRefreshJob). Enqueuing earlier could let the job run before
/// the result it needs exists; a rollback drops the request.
/// </summary>
public interface ISnapshotRefreshRequests
{
    void RequestAfterCommit(Guid userId, Guid attemptId);
}
