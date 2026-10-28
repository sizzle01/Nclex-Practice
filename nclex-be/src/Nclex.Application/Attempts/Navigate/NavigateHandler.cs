using Nclex.Application.Abstractions;
using Nclex.Application.Common;
using Nclex.Application.Common.Validation;
using Nclex.Domain.Entities;

namespace Nclex.Application.Attempts.Navigate;

/// <summary>
/// POST /attempts/{id}/navigate: moves to another item. The server records the time spent on the
/// item being left, so time-per-item never depends on the client (architecture.md 5.2).
/// </summary>
public sealed class NavigateHandler(IAppDbContext db, AttemptStore store, AttemptWriteGuard guard, TimeProvider clock)
{
    public async Task<NavigateResult> HandleAsync(
        Guid attemptId,
        string? clientIdHeader,
        NavigateRequest request,
        CancellationToken cancellationToken)
    {
        var clientId = AttemptClient.Require(clientIdHeader);
        await using var transaction = await db.BeginTransactionAsync(cancellationToken);
        var attempt = await guard.LockForWriteAsync(attemptId, clientId, transaction, cancellationToken);
        if (request.ToPosition > attempt.TotalItems)
        {
            throw AppException.ForField("toPosition", ValidationErrorCodes.InvalidValue);
        }

        var current = await store.CurrentItemAsync(attempt, cancellationToken);
        var target = request.ToPosition == current.Position
            ? current
            : await store.ItemAsync(attempt, request.ToPosition, cancellationToken);

        var now = clock.GetUtcNow();
        if (attempt.Navigate(current, target, now) == NavigationOutcome.BackNavigationDisabled)
        {
            throw new AppException(AttemptErrors.BackNavigationDisabled);
        }

        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return new NavigateResult(attempt.CurrentPosition, TimerState.For(attempt, target, now));
    }
}
