using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Domain.Entities;

namespace Nclex.Application.Catalog.ListTestSets;

/// <summary>GET /test-sets: the curated sets of a track, with each one's lock state (PRD 5.5).</summary>
public sealed class ListTestSetsHandler(IAppDbContext db, PlanTierGuard planTierGuard, LastAttempts lastAttempts)
{
    public async Task<TestSetPage> HandleAsync(ListTestSetsRequest request, CancellationToken cancellationToken)
    {
        var viewer = await planTierGuard.LoadViewerAsync(cancellationToken);
        var track = request.Track ?? viewer.Track;
        var limit = request.Limit ?? ListTestSetsRequest.DefaultLimit;
        var offset = request.Cursor is { } cursor && PageCursor.TryDecode(cursor, out var decoded) ? decoded : 0;

        // One extra row tells us whether there is a next page.
        var sets = await db.TestSets.AsNoTracking()
            .Where(TestSetVisibility.VisibleTo(viewer.UserId))
            .Where(s => s.Track == track && s.Kind == TestSetKind.Prebuilt)
            // Sample sets first, then practice before challenge (the enum's DDL order), then by title.
            .OrderByDescending(s => s.IsSample)
            .ThenBy(s => s.Mode)
            .ThenBy(s => s.Title)
            .ThenBy(s => s.Id)
            .Skip(offset)
            .Take(limit + 1)
            .ToListAsync(cancellationToken);

        var hasMore = sets.Count > limit;
        var page = sets.Take(limit).ToList();
        var last = await lastAttempts.ForSetsAsync(viewer.UserId, [.. page.Select(s => s.Id)], cancellationToken);
        var items = page.Select(s => ToSummary(s, viewer, last.GetValueOrDefault(s.Id))).ToList();
        return new TestSetPage(items, hasMore ? PageCursor.Encode(offset + limit) : null);
    }

    private static TestSetSummary ToSummary(TestSet set, CatalogViewer viewer, LastAttemptSummary? myLastAttempt) => new(
        set.Id,
        set.Title,
        set.Mode,
        set.RevealMode,
        set.QuestionCount,
        set.Timed,
        set.TimeBudgetSec,
        set.PerQuestionSec,
        set.PassThreshold,
        set.IsSample,
        set.AccessTier,
        viewer.IsLocked(set.AccessTier),
        myLastAttempt);
}
