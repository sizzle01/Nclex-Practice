using Nclex.Application.Abstractions;
using Nclex.Application.Common;
using Nclex.Domain.Entities;

namespace Nclex.Application.Attempts.SaveResponse;

/// <summary>
/// PUT /attempts/{id}/items/{position}/response: the autosave (architecture.md 5.2). Idempotent:
/// a write whose <c>clientSeq</c> is not newer than the stored one changes nothing and still gets 200.
/// </summary>
public sealed class SaveResponseHandler(
    IAppDbContext db,
    AttemptStore store,
    AttemptWriteGuard guard,
    ResponseValidator validator,
    NclexMetrics metrics,
    TimeProvider clock)
{
    public async Task<SaveResponseResult> HandleAsync(
        Guid attemptId,
        int position,
        string? clientIdHeader,
        SaveResponseRequest request,
        CancellationToken cancellationToken)
    {
        // Autosave success rate and latency are on-call signals (architecture.md section 9).
        var started = clock.GetTimestamp();
        try
        {
            var (result, outcome) = await SaveAsync(attemptId, position, clientIdHeader, request, cancellationToken);
            metrics.RecordAutosave(clock.GetElapsedTime(started), outcome == SaveOutcome.Saved ? NclexMetrics.AutosaveSaved : NclexMetrics.AutosaveStale);
            return result;
        }
        catch (Exception ex)
        {
            // Expected refusals keep their api.md code; anything else is a server failure.
            var outcome = ex is AppException app ? app.Error.Code : NclexMetrics.AutosaveFailed;
            metrics.RecordAutosave(clock.GetElapsedTime(started), outcome);
            throw;
        }
    }

    private async Task<(SaveResponseResult Result, SaveOutcome Outcome)> SaveAsync(
        Guid attemptId,
        int position,
        string? clientIdHeader,
        SaveResponseRequest request,
        CancellationToken cancellationToken)
    {
        var clientId = AttemptClient.Require(clientIdHeader);
        await using var transaction = await db.BeginTransactionAsync(cancellationToken);
        var attempt = await guard.LockForWriteAsync(attemptId, clientId, transaction, cancellationToken);
        var item = await store.ItemAsync(attempt, position, cancellationToken);
        var response = await validator.ToStoredAsync(item, request.Response, cancellationToken);

        var now = clock.GetUtcNow();
        var outcome = item.SaveResponse(response, request.ClientSeq, now);
        if (outcome == SaveOutcome.ItemLocked)
        {
            // Keeps the lock the guard may have just made because the item's time ran out.
            await guard.CommitAsync(attempt, transaction, cancellationToken);
            throw new AppException(AttemptErrors.ItemLocked);
        }

        if (outcome == SaveOutcome.Saved)
        {
            attempt.RecordActivity(now);
        }

        await guard.CommitAsync(attempt, transaction, cancellationToken);
        var current = await store.CurrentItemAsync(attempt, cancellationToken);
        return (new SaveResponseResult(now, item.ClientSeq, item.State, TimerState.For(attempt, current, now)), outcome);
    }
}
