using Microsoft.EntityFrameworkCore.Storage;
using Nclex.Application.Abstractions;
using Nclex.Application.Attempts.Grading;
using Nclex.Application.Common;
using Nclex.Domain.Entities;

namespace Nclex.Application.Attempts;

/// <summary>
/// The start and end of every attempt write. It starts by locking the row and applying the timer
/// rules before anything else (architecture.md 5.1, rule 1):
/// - Exam time ran out more than 2 s ago: the attempt is finalized and committed in this request,
///   and the write fails with 409 ATTEMPT_TIME_EXPIRED.
/// - The current item's time ran out: that item is locked with its answer. The write carries on,
///   so moving to the next item still works, while a save to the locked item gets ITEM_LOCKED.
/// It ends with <see cref="CommitAsync"/>, which grades what the write locked in tutor mode.
/// </summary>
public sealed class AttemptWriteGuard(
    IAppDbContext db,
    AttemptStore store,
    AttemptFinalizer finalizer,
    IGradingService grading,
    TimeProvider clock)
{
    /// <summary>Locks the attempt, applies the timer rules, then requires the lease of <paramref name="clientId"/>.</summary>
    public async Task<Attempt> LockForWriteAsync(
        Guid attemptId,
        string clientId,
        IDbContextTransaction transaction,
        CancellationToken cancellationToken)
    {
        var attempt = await LockAndApplyTimersAsync(attemptId, transaction, cancellationToken);
        AttemptStore.EnsureWritable(attempt, clientId);
        return attempt;
    }

    /// <summary>
    /// Locks the attempt and applies the timer rules, without the lease check: for open and resume,
    /// which take the lease, and discard, which needs none.
    /// </summary>
    public async Task<Attempt> LockAndApplyTimersAsync(
        Guid attemptId,
        IDbContextTransaction transaction,
        CancellationToken cancellationToken)
    {
        var attempt = await store.LockOwnedAsync(attemptId, cancellationToken);
        var now = clock.GetUtcNow();
        if (attempt.HasExamExpiredAt(now))
        {
            await finalizer.FinalizeAsync(attempt, SubmitReason.TimeExpired, now, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            throw new AppException(AttemptErrors.TimeExpired);
        }

        if (attempt.Status == AttemptStatus.InProgress)
        {
            attempt.ExpireItemTimer(await store.CurrentItemAsync(attempt, cancellationToken), now);
        }

        return attempt;
    }

    /// <summary>
    /// Saves the write and commits. In tutor mode, any item the write locked is graded first, so its
    /// rationale is ready the moment the candidate asks for it (architecture.md 5.5).
    /// </summary>
    public async Task CommitAsync(Attempt attempt, IDbContextTransaction transaction, CancellationToken cancellationToken)
    {
        await grading.GradeLockedTutorItemsAsync(attempt, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
