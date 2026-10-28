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
    TimeProvider clock)
{
    public async Task<SaveResponseResult> HandleAsync(
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
        return new SaveResponseResult(now, item.ClientSeq, item.State, TimerState.For(attempt, current, now));
    }
}
