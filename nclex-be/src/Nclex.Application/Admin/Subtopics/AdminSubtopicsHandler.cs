using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Application.Admin.Questions;
using Nclex.Application.Common;
using Nclex.Domain.Audit;
using Nclex.Domain.Entities;
using Nclex.Domain.ItemTypes;
using Nclex.Domain.Recommendations;

namespace Nclex.Application.Admin.Subtopics;

/// <summary>GET, POST and PATCH /admin/subtopics: the list of topics questions can be tagged with.</summary>
public sealed class AdminSubtopicsHandler(IAppDbContext db, ICurrentUser currentUser, IAuditLog audit, TimeProvider clock)
{
    public async Task<SubtopicList> ListAsync(ListSubtopicsRequest request, CancellationToken cancellationToken)
    {
        var rows = from subtopic in db.Subtopics.AsNoTracking()
                   join category in db.ClientNeedsCategories.AsNoTracking() on subtopic.ClientNeedsId equals category.Id
                   select new { subtopic, category };
        if (request.Track is { } track)
        {
            rows = rows.Where(r => r.category.Track == track);
        }

        if (request.ClientNeedsId is { } clientNeedsId)
        {
            rows = rows.Where(r => r.subtopic.ClientNeedsId == clientNeedsId);
        }

        var items = await rows
            .OrderBy(r => r.category.Track).ThenBy(r => r.category.SortOrder).ThenBy(r => r.subtopic.SortOrder).ThenBy(r => r.subtopic.Name)
            .Select(r => new SubtopicView(
                r.subtopic.Id,
                r.category.Track,
                r.category.Id,
                r.category.Code,
                r.category.Name,
                r.subtopic.Slug,
                r.subtopic.Name,
                r.subtopic.IsActive,
                r.subtopic.SortOrder,
                db.QuestionSubtopics.Count(t => t.SubtopicId == r.subtopic.Id)))
            .ToListAsync(cancellationToken);
        return new SubtopicList(items);
    }

    public async Task<SubtopicView> CreateAsync(CreateSubtopicRequest request, CancellationToken cancellationToken)
    {
        var category = await db.ClientNeedsCategories.AsNoTracking()
            .Where(c => c.Id == request.ClientNeedsId)
            .Select(c => new { c.Track, IsParent = db.ClientNeedsCategories.Any(child => child.ParentId == c.Id) })
            .SingleOrDefaultAsync(cancellationToken);
        if (category is null || category.IsParent)
        {
            throw AuthoringValidator.Invalid([new ValidationIssue("clientNeedsId", ValidationIssueCodes.InvalidValue, "Choose a Client Needs subcategory.")]);
        }

        var name = request.Name.Trim();
        await EnsureNameIsFreeAsync(category.Track, name, null, cancellationToken);

        var subtopic = Subtopic.Create(request.ClientNeedsId, SubtopicNames.Slug(name), name, request.SortOrder, clock.GetUtcNow());
        db.Subtopics.Add(subtopic);
        audit.RecordEntityEvent(AuditActions.SubtopicCreated, currentUser.UserId, AuditActions.SubtopicEntity, subtopic.Id.ToString(), new { subtopic.Slug });
        await db.SaveChangesAsync(cancellationToken);
        return await ReadAsync(subtopic.Id, cancellationToken);
    }

    public async Task<SubtopicView> UpdateAsync(Guid id, UpdateSubtopicRequest request, CancellationToken cancellationToken)
    {
        var subtopic = await db.Subtopics.SingleOrDefaultAsync(s => s.Id == id, cancellationToken)
            ?? throw new AppException(AdminErrors.SubtopicNotFound);
        var name = request.Name?.Trim() ?? subtopic.Name;
        if (name != subtopic.Name)
        {
            var track = await db.ClientNeedsCategories.AsNoTracking()
                .Where(c => c.Id == subtopic.ClientNeedsId)
                .Select(c => c.Track)
                .SingleAsync(cancellationToken);
            await EnsureNameIsFreeAsync(track, name, subtopic.Id, cancellationToken);
        }

        subtopic.ChangeDetails(name, request.SortOrder ?? subtopic.SortOrder);
        if (request.IsActive is { } isActive)
        {
            subtopic.SetActive(isActive);
        }

        audit.RecordEntityEvent(AuditActions.SubtopicUpdated, currentUser.UserId, AuditActions.SubtopicEntity, subtopic.Id.ToString(), new { subtopic.IsActive });
        await db.SaveChangesAsync(cancellationToken);
        return await ReadAsync(subtopic.Id, cancellationToken);
    }

    private async Task<SubtopicView> ReadAsync(Guid id, CancellationToken cancellationToken)
    {
        var all = await ListAsync(new ListSubtopicsRequest(), cancellationToken);
        return all.Items.Single(s => s.Id == id);
    }

    // Names are unique across a track, because candidates see them without their category.
    private async Task EnsureNameIsFreeAsync(ExamTrack track, string name, Guid? ignoredId, CancellationToken cancellationToken)
    {
        var names = await (
            from subtopic in db.Subtopics.AsNoTracking()
            join category in db.ClientNeedsCategories.AsNoTracking() on subtopic.ClientNeedsId equals category.Id
            where category.Track == track && subtopic.Id != ignoredId
            select subtopic.Name)
            .ToListAsync(cancellationToken);
        var key = SubtopicNames.ComparisonKey(name);
        if (names.Any(existing => SubtopicNames.ComparisonKey(existing) == key))
        {
            throw new AppException(AdminErrors.SubtopicDuplicate);
        }
    }
}
