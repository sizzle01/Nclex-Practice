using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Domain.Entities;

namespace Nclex.Application.Catalog;

/// <summary>The <c>myLastAttempt</c> of catalog cards (api.md section 3): the user's latest submitted attempt of each set.</summary>
public sealed class LastAttempts(IAppDbContext db)
{
    /// <summary>The latest submitted attempt per set; sets the user has never finished are left out.</summary>
    public async Task<IReadOnlyDictionary<Guid, LastAttemptSummary>> ForSetsAsync(
        Guid userId,
        IReadOnlyCollection<Guid> setIds,
        CancellationToken cancellationToken)
    {
        var rows = await (
                from attempt in db.Attempts
                join result in db.AttemptResults on attempt.Id equals result.AttemptId
                where attempt.UserId == userId && setIds.Contains(attempt.TestSetId) && attempt.Status == AttemptStatus.Submitted
                select new { attempt.TestSetId, attempt.Id, result.ScorePct, SubmittedAt = attempt.SubmittedAt!.Value })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        // A user has few attempts per set, so picking the latest here is cheaper than a window query.
        return rows
            .GroupBy(r => r.TestSetId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(r => r.SubmittedAt).ThenByDescending(r => r.Id).Select(r => new LastAttemptSummary(r.Id, r.ScorePct, r.SubmittedAt)).First());
    }
}
