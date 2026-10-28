using Nclex.Application.Abstractions;
using Nclex.Domain.Entities;

namespace Nclex.Application.Attempts.Expiry;

/// <summary>
/// The delayed expiry job's work (architecture.md 5.1, rule 2): finalize the attempt if its exam
/// time has run out. It runs without a signed-in user, so it reads the attempt directly.
/// </summary>
public sealed class ExpireAttemptHandler(IAppDbContext db, AttemptFinalizer finalizer, TimeProvider clock)
{
    /// <summary>
    /// True when it finalized the attempt. A job scheduled before a pause or resume carries an old
    /// <paramref name="timerEpoch"/> and does nothing, as does one for an attempt already finished.
    /// </summary>
    public async Task<bool> HandleAsync(Guid attemptId, int timerEpoch, CancellationToken cancellationToken)
    {
        await using var transaction = await db.BeginTransactionAsync(cancellationToken);
        var attempt = await db.LockAttemptAsync(attemptId, cancellationToken);
        var now = clock.GetUtcNow();
        if (attempt is null || attempt.TimerEpoch != timerEpoch || !attempt.HasExamExpiredAt(now))
        {
            return false;
        }

        await finalizer.FinalizeAsync(attempt, SubmitReason.TimeExpired, now, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return true;
    }
}
