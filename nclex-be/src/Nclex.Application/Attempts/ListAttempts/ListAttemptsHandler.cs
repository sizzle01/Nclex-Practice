using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Application.Catalog;
using Nclex.Domain.Entities;

namespace Nclex.Application.Attempts.ListAttempts;

/// <summary>GET /attempts: the user's drafts (dashboard) or past attempts (history), most recent first.</summary>
public sealed class ListAttemptsHandler(IAppDbContext db, ICurrentUser currentUser, TimeProvider clock)
{
    public async Task<AttemptPage> HandleAsync(ListAttemptsRequest request, CancellationToken cancellationToken)
    {
        // The validator has already refused unknown statuses.
        if (!AttemptStatusFilter.TryParse(request.Status, out var statuses))
        {
            statuses = AttemptStatusFilter.Drafts;
        }

        var limit = request.Limit ?? ListAttemptsRequest.DefaultLimit;
        var offset = request.Cursor is { } cursor && PageCursor.TryDecode(cursor, out var decoded) ? decoded : 0;

        // One extra row tells us whether there is a next page.
        var rows = await (
                from attempt in db.Attempts
                join set in db.TestSets on attempt.TestSetId equals set.Id
                where attempt.UserId == currentUser.UserId && statuses.Contains(attempt.Status)
                orderby attempt.LastActivityAt descending, attempt.Id
                select new
                {
                    Attempt = attempt,
                    set.Title,
                    Answered = db.AttemptItems.Count(i => i.AttemptId == attempt.Id && i.Response != null),
                })
            .AsNoTracking()
            .Skip(offset)
            .Take(limit + 1)
            .ToListAsync(cancellationToken);

        var now = clock.GetUtcNow();
        var items = rows.Take(limit).Select(r => new AttemptSummary(
            r.Attempt.Id,
            new TestSetRef(r.Attempt.TestSetId, r.Title),
            r.Attempt.Status,
            r.Attempt.Settings.Mode,
            r.Attempt.TotalItems,
            r.Answered,
            r.Attempt.StartedAt,
            r.Attempt.LastActivityAt,
            r.Attempt.SubmittedAt,
            r.Attempt.IsStaleDraftAt(now),
            r.Attempt.ExamRemainingMs)).ToList();
        return new AttemptPage(items, rows.Count > limit ? PageCursor.Encode(offset + limit) : null);
    }
}
