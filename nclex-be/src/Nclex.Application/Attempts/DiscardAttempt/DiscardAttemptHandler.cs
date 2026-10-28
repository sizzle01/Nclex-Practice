using Nclex.Application.Abstractions;
using Nclex.Application.Common;

namespace Nclex.Application.Attempts.DiscardAttempt;

/// <summary>
/// DELETE /attempts/{id}: discards a draft (status <c>abandoned</c>). It needs no device lease,
/// because the dashboard discards drafts without opening the player. A player still open on
/// another device gets ATTEMPT_NOT_ACTIVE on its next write.
/// </summary>
public sealed class DiscardAttemptHandler(IAppDbContext db, AttemptWriteGuard guard, TimeProvider clock)
{
    public async Task HandleAsync(Guid attemptId, CancellationToken cancellationToken)
    {
        await using var transaction = await db.BeginTransactionAsync(cancellationToken);
        // A draft whose time has run out is finalized here instead, and is kept for its results.
        var attempt = await guard.LockAndApplyTimersAsync(attemptId, transaction, cancellationToken);
        if (!attempt.CanDiscard)
        {
            throw new AppException(AttemptErrors.NotActive);
        }

        attempt.Discard(clock.GetUtcNow());
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
