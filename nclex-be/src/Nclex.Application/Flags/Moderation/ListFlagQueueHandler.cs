using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Application.Notifications.Feed;
using Nclex.Domain.Flags;

namespace Nclex.Application.Flags.Moderation;

/// <summary>
/// GET /admin/flags: the moderation queue for one status, oldest first, so the longest-waiting
/// report is handled next. Open and triaged pages use <c>flags_queue_idx</c>.
/// </summary>
public sealed class ListFlagQueueHandler(IAppDbContext db, FlagQueueReader reader)
{
    public async Task<FlagQueuePage> HandleAsync(ListFlagQueueRequest request, CancellationToken cancellationToken)
    {
        var limit = request.Limit ?? ListFlagQueueRequest.DefaultLimit;
        var status = request.Status ?? FlagStatus.Open;
        var query = db.QuestionFlags.AsNoTracking().Where(f => f.Status == status);

        // The feed's keyset, read the other way: everything after the last row sent.
        if (request.Cursor is { } cursor && FeedCursor.TryDecode(cursor, out var createdAt, out var id))
        {
            query = query.Where(f => f.CreatedAt > createdAt || (f.CreatedAt == createdAt && f.Id > id));
        }

        var rows = await query
            .OrderBy(f => f.CreatedAt)
            .ThenBy(f => f.Id)
            .Take(limit + 1)
            .ToListAsync(cancellationToken);

        var page = rows.Take(limit).ToList();
        var next = rows.Count > limit ? FeedCursor.Encode(page[^1].CreatedAt, page[^1].Id) : null;
        return new FlagQueuePage(await reader.ReadAsync(page, cancellationToken), next);
    }
}
