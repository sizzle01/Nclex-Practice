using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Application.Attempts.GetItem;
using Nclex.Application.Catalog;
using Nclex.Application.Common;
using Nclex.Domain.Entities;

namespace Nclex.Application.Attempts.Review;

/// <summary>
/// GET /attempts/{id}/review: item-by-item rationale review. A submitted attempt shows every item;
/// a tutor-mode attempt in progress shows the items locked so far. Exam mode shows nothing until
/// Submit (architecture.md 5.5).
/// </summary>
public sealed class GetReviewHandler(IAppDbContext db, AttemptStore store, ItemViewReader items)
{
    public async Task<ReviewPage> HandleAsync(Guid attemptId, GetReviewRequest request, CancellationToken cancellationToken)
    {
        var attempt = await store.FindOwnedAsync(attemptId, cancellationToken);
        EnsureReviewable(attempt);

        var submitted = attempt.Status == AttemptStatus.Submitted;
        var query = db.AttemptItems.AsNoTracking()
            .Where(i => i.AttemptId == attempt.Id && i.Position >= (request.Position ?? 1));
        if (!submitted)
        {
            query = query.Where(i => i.State == ItemState.Locked);
        }

        query = request.Filter switch
        {
            ReviewFilter.Incorrect => query.Where(i => i.IsCorrect == false),
            ReviewFilter.Flagged => query.Where(i => i.Flagged),
            _ => query,
        };

        var limit = request.Limit ?? GetReviewRequest.DefaultLimit;
        var offset = request.Cursor is { } cursor && PageCursor.TryDecode(cursor, out var decoded) ? decoded : 0;
        // One extra row tells us whether there is a next page.
        var positions = await query
            .OrderBy(i => i.Position)
            .Select(i => i.Position)
            .Skip(offset)
            .Take(limit + 1)
            .ToListAsync(cancellationToken);

        var views = await items.ReadAsync(attempt, [.. positions.Take(limit)], cancellationToken);
        return new ReviewPage(views, positions.Count > limit ? PageCursor.Encode(offset + limit) : null);
    }

    private static void EnsureReviewable(Attempt attempt)
    {
        switch (attempt.Status)
        {
            case AttemptStatus.Submitted:
                return;
            case AttemptStatus.Abandoned:
                throw new AppException(AttemptErrors.NotActive);
            case AttemptStatus.Paused:
                // Content is hidden during a pause (PRD 5.4).
                throw new AppException(AttemptErrors.Paused);
        }

        if (attempt.Settings.RevealMode != RevealMode.Tutor)
        {
            throw new AppException(AttemptErrors.NotSubmitted);
        }
    }
}
