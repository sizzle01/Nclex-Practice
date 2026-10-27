using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Application.Common;
using Nclex.Application.Common.Validation;
using Nclex.Domain.Builder;
using Nclex.Domain.Entities;

namespace Nclex.Application.Catalog.CustomSets;

/// <summary>The questions a custom set may be built from, and the selected leaves with their weights.</summary>
/// <param name="Track">The track the filters resolved to.</param>
/// <param name="Items">Eligible questions; case studies are complete.</param>
/// <param name="SelectedLeaves">The Client Needs leaves chosen (parents expanded); empty means any.</param>
public sealed record LoadedPool(ExamTrack Track, IReadOnlyList<PoolItem> Items, IReadOnlyList<LeafWeight> SelectedLeaves);

/// <summary>
/// Step 1 of the custom builder (architecture.md 5.7): the candidate pool, in hand-written SQL.
/// It takes the current published version of each active question on the track and the user's
/// tier, then applies the tag filters and the pool mode.
/// </summary>
public sealed class QuestionPool(IAppDbContext db)
{
    public async Task<LoadedPool> LoadAsync(IPoolFilters filters, CatalogViewer viewer, CancellationToken cancellationToken)
    {
        var track = filters.Track ?? viewer.Track;
        var leaves = await ExpandToLeavesAsync(track, filters.ClientNeedsIds ?? [], cancellationToken);
        await EnsureBodySystemsExistAsync(filters.BodySystemIds ?? [], cancellationToken);

        var rows = await QueryAsync(filters, viewer, track, [.. leaves.Select(l => l.ClientNeedsId)], cancellationToken);
        var items = await DropIncompleteCaseStudiesAsync(rows, cancellationToken);
        return new LoadedPool(track, items, leaves);
    }

    private Task<List<PoolItem>> QueryAsync(
        IPoolFilters filters,
        CatalogViewer viewer,
        ExamTrack track,
        short[] leafIds,
        CancellationToken cancellationToken)
    {
        // Enums travel as text and are cast in SQL, so the query doesn't depend on parameter type mapping.
        var trackName = track.ToString();
        var includePremium = viewer.Plan == PlanTier.Premium;
        var bodySystemIds = (filters.BodySystemIds ?? []).ToArray();
        var itemTypes = (filters.ItemTypes ?? []).Select(SnakeCase).ToArray();
        var difficulties = (filters.Difficulties ?? []).Select(SnakeCase).ToArray();
        var pool = SnakeCase(filters.Pool ?? PoolMode.All);
        var userId = viewer.UserId;

        // An empty array means "any". The qv_filter_idx partial index covers the tag columns.
        return db.SqlQuery<PoolItem>(
            $"""
            SELECT q.id AS "QuestionId", v.client_needs_id AS "ClientNeedsId",
                   v.case_study_id AS "CaseStudyId", v.case_position AS "CasePosition"
            FROM questions q
            JOIN question_versions v ON v.id = q.current_version_id
            WHERE q.track = {trackName}::exam_track
              AND q.status = 'active'
              AND v.status = 'published'
              AND (q.access_tier = 'free' OR {includePremium})
              AND (cardinality({leafIds}) = 0 OR v.client_needs_id = ANY({leafIds}))
              AND (cardinality({bodySystemIds}) = 0 OR v.body_system_id = ANY({bodySystemIds}))
              AND (cardinality({itemTypes}) = 0 OR v.item_type::text = ANY({itemTypes}))
              AND (cardinality({difficulties}) = 0 OR v.difficulty::text = ANY({difficulties}))
              AND CASE {pool}
                    WHEN 'unused' THEN NOT EXISTS (
                      SELECT 1 FROM user_question_history h WHERE h.user_id = {userId} AND h.question_id = q.id)
                    WHEN 'incorrect' THEN EXISTS (
                      SELECT 1 FROM user_question_history h
                      WHERE h.user_id = {userId} AND h.question_id = q.id AND h.last_correct = false)
                    WHEN 'flagged' THEN false -- needs attempt_items.flagged (F08)
                    ELSE true
                  END
            """)
            .ToListAsync(cancellationToken);
    }

    // A parent category stands for all of its leaves; questions are only ever tagged to leaves.
    private async Task<List<LeafWeight>> ExpandToLeavesAsync(ExamTrack track, IReadOnlyList<short> ids, CancellationToken cancellationToken)
    {
        if (ids.Count == 0)
        {
            return [];
        }

        var categories = await db.ClientNeedsCategories.AsNoTracking()
            .Where(c => c.Track == track)
            .ToListAsync(cancellationToken);
        var byId = categories.ToDictionary(c => c.Id);
        if (ids.Any(id => !byId.ContainsKey(id)))
        {
            throw AppException.ForField("clientNeedsIds", ValidationErrorCodes.InvalidValue);
        }

        var childrenByParent = categories.Where(c => c.ParentId is not null).ToLookup(c => c.ParentId!.Value);
        return [.. ids
            .SelectMany(id => childrenByParent[id].DefaultIfEmpty(byId[id]))
            .DistinctBy(c => c.Id)
            .OrderBy(c => c.Id)
            .Select(c => new LeafWeight(c.Id, Midpoint(c)))];
    }

    // The test plan gives each leaf a range, e.g. 15 to 21%; its middle is the leaf's weight.
    private static decimal Midpoint(ClientNeedsCategory leaf) =>
        ((leaf.WeightMin ?? 0) + (leaf.WeightMax ?? 0)) / 2;

    private async Task EnsureBodySystemsExistAsync(IReadOnlyList<short> ids, CancellationToken cancellationToken)
    {
        if (ids.Count == 0)
        {
            return;
        }

        var distinct = ids.Distinct().ToList();
        var found = await db.BodySystems.CountAsync(b => distinct.Contains(b.Id), cancellationToken);
        if (found != distinct.Count)
        {
            throw AppException.ForField("bodySystemIds", ValidationErrorCodes.InvalidValue);
        }
    }

    // A set never holds part of a case study: if a filter leaves any of its items out, the whole
    // case study goes.
    private async Task<IReadOnlyList<PoolItem>> DropIncompleteCaseStudiesAsync(List<PoolItem> rows, CancellationToken cancellationToken)
    {
        var caseIds = rows.Where(r => r.CaseStudyId is not null).Select(r => r.CaseStudyId!.Value).Distinct().ToList();
        if (caseIds.Count == 0)
        {
            return rows;
        }

        var itemsPerCase = await db.Questions
            .Where(q => q.Status == QuestionStatus.Active)
            .Join(db.QuestionVersions, q => q.CurrentVersionId, v => v.Id, (_, v) => v)
            .Where(v => v.Status == VersionStatus.Published && v.CaseStudyId != null && caseIds.Contains(v.CaseStudyId.Value))
            .GroupBy(v => v.CaseStudyId!.Value)
            .Select(g => new { CaseStudyId = g.Key, Items = g.Count() })
            .ToDictionaryAsync(x => x.CaseStudyId, x => x.Items, cancellationToken);
        var inPool = rows.Where(r => r.CaseStudyId is not null).GroupBy(r => r.CaseStudyId!.Value)
            .ToDictionary(g => g.Key, g => g.Count());

        return [.. rows.Where(r => r.CaseStudyId is not { } caseId || inPool[caseId] == itemsPerCase.GetValueOrDefault(caseId))];
    }

    private static string SnakeCase<TEnum>(TEnum value)
        where TEnum : struct, Enum =>
        JsonNamingPolicy.SnakeCaseLower.ConvertName(value.ToString());
}
