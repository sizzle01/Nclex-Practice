using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Domain.Entities;
using Nclex.Domain.Timing;

namespace Nclex.Application.Attempts.Expiry;

/// <summary>
/// The sweeper's work, run every minute (architecture.md 5.1, rule 3): finalize running attempts
/// more than 30 s past their deadline, covering any delayed job that was lost or failed. Paused
/// attempts have no deadline, so drafts are never swept.
/// </summary>
public sealed class SweepExpiredAttemptsHandler(IAppDbContext db, AttemptFinalizer finalizer, TimeProvider clock)
{
    /// <summary>Returns how many attempts it finalized.</summary>
    public async Task<int> HandleAsync(CancellationToken cancellationToken)
    {
        var cutoff = AttemptTimer.SweepCutoff(clock.GetUtcNow());
        // Served by attempts_expiry_idx (database.md 2.6).
        var overdue = await db.Attempts.AsNoTracking()
            .Where(a => a.Status == AttemptStatus.InProgress && a.ExamDeadlineAt < cutoff)
            .Select(a => a.Id)
            .ToListAsync(cancellationToken);

        var finalized = 0;
        foreach (var attemptId in overdue)
        {
            // One transaction per attempt, so one slow row never holds the others' locks.
            if (await FinalizeIfStillExpiredAsync(attemptId, cancellationToken))
            {
                finalized++;
            }
        }

        return finalized;
    }

    private async Task<bool> FinalizeIfStillExpiredAsync(Guid attemptId, CancellationToken cancellationToken)
    {
        await using var transaction = await db.BeginTransactionAsync(cancellationToken);
        var attempt = await db.LockAttemptAsync(attemptId, cancellationToken);
        var now = clock.GetUtcNow();
        // A write or the delayed job may have finalized it since the query above.
        if (attempt is null || !attempt.HasExamExpiredAt(now))
        {
            return false;
        }

        await finalizer.FinalizeAsync(attempt, SubmitReason.TimeExpired, now, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return true;
    }
}
