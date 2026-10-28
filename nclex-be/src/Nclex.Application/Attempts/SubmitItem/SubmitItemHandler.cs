using Nclex.Application.Abstractions;
using Nclex.Application.Attempts.Reveal;
using Nclex.Application.Common;
using Nclex.Domain.Entities;

namespace Nclex.Application.Attempts.SubmitItem;

/// <summary>
/// POST /attempts/{id}/items/{position}/submit: tutor mode's "Submit" on one item. It locks the
/// item, grades it and returns the reveal (architecture.md 5.5). Submitting an item that is
/// already locked changes nothing and returns its reveal again, so a retried request is harmless.
/// </summary>
public sealed class SubmitItemHandler(
    IAppDbContext db,
    AttemptStore store,
    AttemptWriteGuard guard,
    ResponseValidator validator,
    RevealReader reveals,
    TimeProvider clock)
{
    public async Task<SubmitItemResult> HandleAsync(
        Guid attemptId,
        int position,
        string? clientIdHeader,
        SubmitItemRequest request,
        CancellationToken cancellationToken)
    {
        var clientId = AttemptClient.Require(clientIdHeader);
        await using var transaction = await db.BeginTransactionAsync(cancellationToken);
        var attempt = await guard.LockForWriteAsync(attemptId, clientId, transaction, cancellationToken);
        if (attempt.Settings.RevealMode != RevealMode.Tutor)
        {
            throw new AppException(AttemptErrors.TutorModeOnly);
        }

        var item = await store.ItemAsync(attempt, position, cancellationToken);
        if (!item.IsLocked)
        {
            var now = clock.GetUtcNow();
            if (request.ClientSeq is { } clientSeq)
            {
                var response = await validator.ToStoredAsync(item, request.Response, cancellationToken);
                item.SaveResponse(response, clientSeq, now);
            }

            attempt.SubmitItem(item, now);
        }

        // Grades the item (and any the timers locked) before the commit.
        await guard.CommitAsync(attempt, transaction, cancellationToken);
        var revealed = await reveals.ReadAsync(attempt, [item], cancellationToken);
        return new SubmitItemResult(revealed[item.Position]);
    }
}
