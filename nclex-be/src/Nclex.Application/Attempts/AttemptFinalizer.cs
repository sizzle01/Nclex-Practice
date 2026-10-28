using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Application.Attempts.Grading;
using Nclex.Domain.Entities;

namespace Nclex.Application.Attempts;

/// <summary>
/// Ends an attempt: marks it submitted, locks every item with its current answer, then grades it and
/// writes the result, all in the caller's transaction (architecture.md 5.5). The one place an attempt
/// is finalized, whichever path got there (Submit, a write after the deadline, the delayed job, the
/// sweeper, an overrun pause).
/// </summary>
public sealed class AttemptFinalizer(IAppDbContext db, IGradingService grading)
{
    /// <summary>
    /// Finalizes <paramref name="attempt"/>, which the caller has locked FOR UPDATE inside its
    /// transaction. The caller saves and commits, so the result can never exist without the
    /// submitted attempt, or the other way round.
    /// </summary>
    public async Task<AttemptResult> FinalizeAsync(Attempt attempt, SubmitReason reason, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var items = await db.AttemptItems
            .Where(i => i.AttemptId == attempt.Id)
            .OrderBy(i => i.Position)
            .ToListAsync(cancellationToken);

        // Finalizing locks unseen items too, so note now which ones the candidate actually saw.
        var seenPositions = items.Where(i => i.State != ItemState.Unseen).Select(i => i.Position).ToHashSet();
        attempt.Finalize(reason, items, now);
        return await grading.FinalizeAttemptAsync(attempt, items, seenPositions, cancellationToken);
    }
}
