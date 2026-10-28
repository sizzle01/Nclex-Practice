using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Domain.Entities;

namespace Nclex.Application.Attempts;

/// <summary>
/// Ends an attempt: marks it submitted and locks every item with its current answer. The one place
/// an attempt is finalized, whichever path got there (a write after the deadline, the delayed job,
/// the sweeper, an overrun pause).
/// </summary>
public sealed class AttemptFinalizer(IAppDbContext db)
{
    /// <summary>
    /// Finalizes <paramref name="attempt"/>, which the caller has locked FOR UPDATE inside its
    /// transaction. The caller saves and commits.
    /// </summary>
    public async Task FinalizeAsync(Attempt attempt, SubmitReason reason, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var items = await db.AttemptItems
            .Where(i => i.AttemptId == attempt.Id)
            .OrderBy(i => i.Position)
            .ToListAsync(cancellationToken);
        attempt.Finalize(reason, items, now);

        // F10 hook: grade the locked items and write attempt_results here, in the same transaction
        // (IGradingService.FinalizeAttemptAsync, interfaces.md section 3). Until then a finalized
        // attempt has no score.
    }
}
