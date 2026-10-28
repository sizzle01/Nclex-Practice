using Nclex.Application.Abstractions;
using Nclex.Application.Common;

namespace Nclex.Application.Attempts.UpdateItemMarks;

/// <summary>PATCH /attempts/{id}/items/{position}: mark for review and strike-through.</summary>
public sealed class UpdateItemMarksHandler(IAppDbContext db, AttemptStore store, AttemptWriteGuard guard, TimeProvider clock)
{
    public async Task<ItemMarks> HandleAsync(
        Guid attemptId,
        int position,
        string? clientIdHeader,
        UpdateItemMarksRequest request,
        CancellationToken cancellationToken)
    {
        var clientId = AttemptClient.Require(clientIdHeader);
        await using var transaction = await db.BeginTransactionAsync(cancellationToken);
        var attempt = await guard.LockForWriteAsync(attemptId, clientId, transaction, cancellationToken);
        var item = await store.ItemAsync(attempt, position, cancellationToken);

        if (!item.UpdateMarks(request.Flagged, request.StruckOptions))
        {
            // Keeps the lock the guard may have just made because the item's time ran out.
            await guard.CommitAsync(attempt, transaction, cancellationToken);
            throw new AppException(AttemptErrors.ItemLocked);
        }

        attempt.RecordActivity(clock.GetUtcNow());
        await guard.CommitAsync(attempt, transaction, cancellationToken);
        return new ItemMarks(item.Position, item.Flagged, item.StruckOptions);
    }
}
