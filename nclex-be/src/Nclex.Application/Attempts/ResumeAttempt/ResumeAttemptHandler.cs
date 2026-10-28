using Nclex.Application.Abstractions;
using Nclex.Application.Common;
using Nclex.Domain.Entities;
using Nclex.Domain.Timing;

namespace Nclex.Application.Attempts.ResumeAttempt;

/// <summary>
/// POST /attempts/{id}/resume: ends a pause on this device, which takes the lease (architecture.md
/// 5.3). Time past the pause limit has come off the exam time; if that leaves none, the attempt is
/// finalized as time_expired and the call fails with ATTEMPT_TIME_EXPIRED. Resuming an attempt
/// that is already running just takes the lease, like /open.
/// </summary>
public sealed class ResumeAttemptHandler(
    IAppDbContext db,
    AttemptStore store,
    AttemptWriteGuard guard,
    AttemptFinalizer finalizer,
    AttemptStateReader stateReader,
    IJobScheduler jobs,
    TimeProvider clock)
{
    public async Task<AttemptState> HandleAsync(Guid attemptId, string? clientIdHeader, CancellationToken cancellationToken)
    {
        var clientId = AttemptClient.Require(clientIdHeader);
        await using var transaction = await db.BeginTransactionAsync(cancellationToken);
        var attempt = await guard.LockAndApplyTimersAsync(attemptId, transaction, cancellationToken);
        AttemptStore.EnsureDraft(attempt);

        var now = clock.GetUtcNow();
        var current = await store.CurrentItemAsync(attempt, cancellationToken);
        if (attempt.Status == AttemptStatus.InProgress)
        {
            attempt.Open(clientId, current, now);
        }
        else if (attempt.Resume(clientId, current, now) == ResumeOutcome.TimeExpired)
        {
            await finalizer.FinalizeAsync(attempt, SubmitReason.TimeExpired, now, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            throw new AppException(AttemptErrors.TimeExpired);
        }

        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        // The resume moved the epoch on, so the job scheduled before the pause will do nothing.
        if (attempt.ExamDeadlineAt is { } deadline)
        {
            jobs.ScheduleAttemptExpiry(attempt.Id, attempt.TimerEpoch, AttemptTimer.ExpiryJobAt(deadline));
        }

        return await stateReader.ReadAsync(attempt, cancellationToken);
    }
}
