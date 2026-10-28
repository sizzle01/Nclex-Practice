using Nclex.Application.Abstractions;
using Nclex.Application.Common;
using Nclex.Domain.Entities;

namespace Nclex.Application.Attempts.PauseAttempt;

/// <summary>
/// POST /attempts/{id}/pause: a break or Save &amp; Exit (architecture.md 5.1). Both freeze the timers
/// and count against the set's pause limit. Pausing an attempt this device already paused returns
/// it unchanged, so a retried request does not use up a second pause.
/// </summary>
public sealed class PauseAttemptHandler(
    IAppDbContext db,
    AttemptStore store,
    AttemptWriteGuard guard,
    TimeProvider clock)
{
    public async Task<PauseAttemptResult> HandleAsync(
        Guid attemptId,
        string? clientIdHeader,
        PauseAttemptRequest request,
        CancellationToken cancellationToken)
    {
        var clientId = AttemptClient.Require(clientIdHeader);
        await using var transaction = await db.BeginTransactionAsync(cancellationToken);
        var attempt = await guard.LockAndApplyTimersAsync(attemptId, transaction, cancellationToken);
        var now = clock.GetUtcNow();
        var current = await store.CurrentItemAsync(attempt, cancellationToken);
        if (attempt.Status == AttemptStatus.Paused && attempt.ActiveClientId == clientId)
        {
            return new PauseAttemptResult(attempt.Status, TimerState.For(attempt, current, now));
        }

        AttemptStore.EnsureWritable(attempt, clientId);
        if (attempt.Pause(request.Reason!.Value, current, now) == PauseOutcome.LimitReached)
        {
            throw new AppException(AttemptErrors.PauseLimitReached);
        }

        await guard.CommitAsync(attempt, transaction, cancellationToken);
        return new PauseAttemptResult(attempt.Status, TimerState.For(attempt, current, now));
    }
}
