using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Application.Notifications.Feed;

namespace Nclex.Application.Flags.MyFlags;

/// <summary>GET /me/flags: the signed-in user's reports and where each one stands, newest first.</summary>
public sealed class ListMyFlagsHandler(IAppDbContext db, ICurrentUser currentUser)
{
    public async Task<MyFlagPage> HandleAsync(ListMyFlagsRequest request, CancellationToken cancellationToken)
    {
        var limit = request.Limit ?? ListMyFlagsRequest.DefaultLimit;
        var userId = currentUser.UserId;
        var query = db.QuestionFlags.AsNoTracking()
            .Where(f => f.UserId == userId)
            .Join(db.QuestionVersions, f => f.QuestionVersionId, v => v.Id, (f, v) => new { Flag = f, v.Stem });

        // A keyset like the notification feed's: new reports arrive at the top while paging down.
        if (request.Cursor is { } cursor && FeedCursor.TryDecode(cursor, out var createdAt, out var id))
        {
            query = query.Where(r => r.Flag.CreatedAt < createdAt || (r.Flag.CreatedAt == createdAt && r.Flag.Id < id));
        }

        var rows = await query
            .OrderByDescending(r => r.Flag.CreatedAt)
            .ThenByDescending(r => r.Flag.Id)
            .Take(limit + 1)
            .ToListAsync(cancellationToken);

        var page = rows.Take(limit).ToList();
        var next = rows.Count > limit ? FeedCursor.Encode(page[^1].Flag.CreatedAt, page[^1].Flag.Id) : null;
        return new MyFlagPage([.. page.Select(r => MyFlagView.From(r.Flag, r.Stem))], next);
    }
}
