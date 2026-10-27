using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Application.Common;
using Nclex.Domain.Entities;

namespace Nclex.Application.Attempts;

/// <summary>
/// Loads the signed-in user's attempts and their items. Someone else's attempt is reported as not
/// found, so its id does not even confirm that it exists.
/// </summary>
public sealed class AttemptStore(IAppDbContext db, ICurrentUser currentUser)
{
    /// <summary>
    /// Locks the attempt row with <c>SELECT … FOR UPDATE</c> and returns it tracked. Every change to an
    /// attempt or its items starts here, inside a transaction, so writes to one attempt run one at a
    /// time (database.md section 2.6 invariants).
    /// </summary>
    public async Task<Attempt> LockOwnedAsync(Guid attemptId, CancellationToken cancellationToken) =>
        OwnedOrNotFound(await db.LockAttemptAsync(attemptId, cancellationToken));

    /// <summary>Reads the attempt without locking it, for GET requests.</summary>
    public async Task<Attempt> FindOwnedAsync(Guid attemptId, CancellationToken cancellationToken) =>
        OwnedOrNotFound(await db.Attempts.AsNoTracking().SingleOrDefaultAsync(a => a.Id == attemptId, cancellationToken));

    /// <summary>The item at <paramref name="position"/>, tracked for changes; 404 when there is none.</summary>
    public async Task<AttemptItem> ItemAsync(Attempt attempt, int position, CancellationToken cancellationToken) =>
        await db.AttemptItems.SingleOrDefaultAsync(i => i.AttemptId == attempt.Id && i.Position == position, cancellationToken)
        ?? throw new AppException(AttemptErrors.ItemNotFound);

    /// <summary>Stops with the matching api.md error unless <paramref name="clientId"/> may write to the attempt now.</summary>
    public static void EnsureWritable(Attempt attempt, string clientId)
    {
        var error = attempt.CheckWrite(clientId) switch
        {
            AttemptWriteCheck.NotActive => AttemptErrors.NotActive,
            AttemptWriteCheck.Paused => AttemptErrors.Paused,
            AttemptWriteCheck.OpenedElsewhere => AttemptErrors.OpenedElsewhere,
            _ => null,
        };
        if (error is not null)
        {
            throw new AppException(error);
        }
    }

    private Attempt OwnedOrNotFound(Attempt? attempt) =>
        attempt is not null && attempt.UserId == currentUser.UserId
            ? attempt
            : throw new AppException(AttemptErrors.AttemptNotFound);
}
