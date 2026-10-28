using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Nclex.Application.Abstractions;

namespace Nclex.Infrastructure.Jobs;

/// <summary>
/// Queues SnapshotRefreshJob right after a submit commits (architecture.md 5.5: COMMIT, then
/// enqueue). A use case with its own transaction commits in <c>CommitAsync</c>; one without commits
/// inside <c>SaveChangesAsync</c>. A rollback or a failed save drops the request.
/// </summary>
/// <remarks>
/// If the process dies between the commit and the enqueue, the job for that attempt is lost; the
/// next submit's job adds the missing attempt too (see SnapshotCatchUp), and an admin rebuild
/// repairs a user who never submits again. Only the async hooks are implemented: every use case
/// saves asynchronously.
/// </remarks>
internal sealed class EnqueueSnapshotRefreshAfterCommit(PendingSnapshotRefreshes pending, IJobScheduler jobs)
    : IDbTransactionInterceptor, ISaveChangesInterceptor
{
    public ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context?.Database.CurrentTransaction is null)
        {
            EnqueuePending();
        }

        return ValueTask.FromResult(result);
    }

    public Task SaveChangesFailedAsync(DbContextErrorEventData eventData, CancellationToken cancellationToken = default)
    {
        pending.Discard();
        return Task.CompletedTask;
    }

    public Task TransactionCommittedAsync(
        DbTransaction transaction,
        TransactionEndEventData eventData,
        CancellationToken cancellationToken = default)
    {
        EnqueuePending();
        return Task.CompletedTask;
    }

    public Task TransactionRolledBackAsync(
        DbTransaction transaction,
        TransactionEndEventData eventData,
        CancellationToken cancellationToken = default)
    {
        pending.Discard();
        return Task.CompletedTask;
    }

    private void EnqueuePending()
    {
        foreach (var (userId, attemptId) in pending.TakeAll())
        {
            jobs.EnqueueSnapshotRefresh(userId, attemptId);
        }
    }
}
