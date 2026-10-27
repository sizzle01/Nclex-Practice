using Nclex.Application.Abstractions;
using Nclex.Application.Common;

namespace Nclex.Application.Attempts.UpdateItemMarks;

/// <summary>PATCH /attempts/{id}/items/{position}: mark for review and strike-through.</summary>
public sealed class UpdateItemMarksHandler(IAppDbContext db, AttemptStore store, TimeProvider clock)
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
        var attempt = await store.LockOwnedAsync(attemptId, cancellationToken);
        AttemptStore.EnsureWritable(attempt, clientId);
        var item = await store.ItemAsync(attempt, position, cancellationToken);

        if (!item.UpdateMarks(request.Flagged, request.StruckOptions))
        {
            throw new AppException(AttemptErrors.ItemLocked);
        }

        attempt.RecordActivity(clock.GetUtcNow());
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return new ItemMarks(item.Position, item.Flagged, item.StruckOptions);
    }
}
