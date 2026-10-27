using Nclex.Application.Abstractions;
using Nclex.Application.Common;

namespace Nclex.Application.Attempts.OpenAttempt;

/// <summary>
/// POST /attempts/{id}/open: the player loaded, so its client id takes the device lease from any
/// other device (architecture.md 5.2). This is how "Continue here" works.
/// </summary>
public sealed class OpenAttemptHandler(
    IAppDbContext db,
    AttemptStore store,
    AttemptStateReader stateReader,
    TimeProvider clock)
{
    public async Task<AttemptState> HandleAsync(Guid attemptId, string? clientIdHeader, CancellationToken cancellationToken)
    {
        var clientId = AttemptClient.Require(clientIdHeader);
        await using var transaction = await db.BeginTransactionAsync(cancellationToken);
        var attempt = await store.LockOwnedAsync(attemptId, cancellationToken);
        if (!attempt.IsDraft)
        {
            throw new AppException(AttemptErrors.NotActive);
        }

        var current = await store.ItemAsync(attempt, attempt.CurrentPosition, cancellationToken);
        attempt.Open(clientId, current, clock.GetUtcNow());
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return await stateReader.ReadAsync(attempt, cancellationToken);
    }
}
