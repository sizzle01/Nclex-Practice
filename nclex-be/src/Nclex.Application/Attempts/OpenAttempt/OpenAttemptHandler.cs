using Nclex.Application.Abstractions;
using Nclex.Domain.Timing;

namespace Nclex.Application.Attempts.OpenAttempt;

/// <summary>
/// POST /attempts/{id}/open: the player loaded, so its client id takes the device lease from any
/// other device (architecture.md 5.2). This is how "Continue here" works. An attempt in progress
/// starts or keeps running its timers; a paused one stays paused behind the overlay.
/// </summary>
public sealed class OpenAttemptHandler(
    IAppDbContext db,
    AttemptStore store,
    AttemptWriteGuard guard,
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

        var current = await store.CurrentItemAsync(attempt, cancellationToken);
        attempt.Open(clientId, current, clock.GetUtcNow());
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        // The timer starts here on the first open (architecture.md 5.1, rule 2). Scheduling again on a
        // later open is harmless: whichever job runs second finds the attempt already finalized.
        if (attempt.ExamDeadlineAt is { } deadline)
        {
            jobs.ScheduleAttemptExpiry(attempt.Id, attempt.TimerEpoch, AttemptTimer.ExpiryJobAt(deadline));
        }

        return await stateReader.ReadAsync(attempt, cancellationToken);
    }
}
