using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Domain.Entities;

namespace Nclex.Application.Admin.Questions.Search;

/// <summary>
/// GET /admin/questions: search for staff. One row per question, described by its latest version.
/// The reviewer queue (status=in_review) lists the oldest submission first, so nothing waits long;
/// every other list shows the most recently changed first.
/// </summary>
public sealed class SearchQuestionsHandler(IAppDbContext db)
{
    // ToLower() here is translated to SQL lower(); it never runs in .NET, so the culture rules don't apply.
    // (Npgsql can't translate ToLowerInvariant.)
    [SuppressMessage("Globalization", "CA1304", Justification = "Translated to SQL lower() by EF Core.")]
    [SuppressMessage("Globalization", "CA1311", Justification = "Translated to SQL lower() by EF Core.")]
    public async Task<QuestionSearchPage> HandleAsync(SearchQuestionsRequest request, CancellationToken cancellationToken)
    {
        var (limit, offset) = AdminPaging.Read(request.Limit, request.Cursor);

        var rows = from v in db.QuestionVersions.AsNoTracking()
                   where v.Version == db.QuestionVersions.Where(o => o.QuestionId == v.QuestionId).Max(o => o.Version)
                   join q in db.Questions.AsNoTracking() on v.QuestionId equals q.Id
                   join author in db.Users.AsNoTracking() on v.AuthorId equals author.Id
                   select new { Version = v, Question = q, AuthorName = author.DisplayName ?? author.Email };

        if (request.Track is { } track)
        {
            rows = rows.Where(r => r.Question.Track == track);
        }

        if (request.QuestionStatus is { } questionStatus)
        {
            rows = rows.Where(r => r.Question.Status == questionStatus);
        }

        rows = request.Status switch
        {
            null => rows,
            VersionStatus.Published => rows.Where(r => r.Question.CurrentVersionId != null),
            var status => rows.Where(r => r.Version.Status == status),
        };

        if (request.ItemType is { } itemType)
        {
            rows = rows.Where(r => r.Version.ItemType == itemType);
        }

        if (request.ClientNeedsId is { } clientNeedsId)
        {
            rows = rows.Where(r => r.Version.ClientNeedsId == clientNeedsId);
        }

        if (!string.IsNullOrWhiteSpace(request.Q))
        {
            var text = request.Q.Trim();
            var pattern = SearchPattern.Containing(text);
            rows = Guid.TryParse(text, out var id)
                ? rows.Where(r => r.Question.Id == id)
                : rows.Where(r => EF.Functions.Like(r.Version.Stem.ToLower(), pattern, SearchPattern.Escape));
        }

        rows = request.Status == VersionStatus.InReview
            ? rows.OrderBy(r => r.Version.CreatedAt).ThenBy(r => r.Question.Id)
            : rows.OrderByDescending(r => r.Version.CreatedAt).ThenBy(r => r.Question.Id);

        // One extra row tells us whether there is a next page.
        var found = await rows
            .Skip(offset)
            .Take(limit + 1)
            .Select(r => new QuestionSummary(
                r.Question.Id,
                r.Question.Track,
                r.Question.Status,
                r.Question.AccessTier,
                r.Version.Id,
                r.Version.Version,
                r.Version.Status,
                r.Version.ItemType,
                r.Version.Stem,
                r.Version.ClientNeedsId,
                r.Version.BodySystemId,
                r.Version.Difficulty,
                r.Version.AuthorId,
                r.AuthorName,
                db.QuestionVersions.Where(p => p.Id == r.Question.CurrentVersionId).Select(p => (int?)p.Version).FirstOrDefault(),
                r.Version.CreatedAt))
            .ToListAsync(cancellationToken);

        var (items, next) = AdminPaging.Page(found, limit, offset);
        return new QuestionSearchPage(items, next);
    }
}
