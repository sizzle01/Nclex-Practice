using System.Globalization;
using System.Text.Json;
using Nclex.Application.Abstractions;
using Nclex.Application.Attempts.StartAttempt;
using Nclex.Application.Catalog;
using Nclex.Application.Catalog.CustomSets;
using Nclex.Application.Common;
using Nclex.Domain.Builder;
using Nclex.Domain.Entities;
using Nclex.Domain.ItemTypes;
using Nclex.Domain.WeakAreas;

namespace Nclex.Application.WeakAreas.StartWeakAreaPractice;

/// <summary>
/// POST /test-sets/weak-areas (architecture.md 5.7.1): one click builds a tutor-mode practice test
/// from the user's weak areas and starts it. The set is an ordinary custom set, so the player,
/// results and history treat it like any other; no new tables are needed.
/// </summary>
public sealed class StartWeakAreaPracticeHandler(
    IAppDbContext db,
    PlanTierGuard planTierGuard,
    WeakAreaFinder finder,
    WeakAreaPool pool,
    StartAttemptHandler startAttempt,
    TimeProvider clock)
{
    public async Task<CreateCustomSetResponse> HandleAsync(StartWeakAreaPracticeRequest request, CancellationToken cancellationToken)
    {
        var viewer = await planTierGuard.LoadViewerAsync(cancellationToken);
        var track = request.Track ?? viewer.Track;
        var found = await finder.FindAsync(viewer.UserId, track, cancellationToken);
        if (found.Areas.Count == 0)
        {
            throw new AppException(WeakAreaErrors.NoWeakAreas);
        }

        var loaded = await pool.LoadAsync(viewer, track, found.Areas, cancellationToken);
        var count = WeakAreaPractice.SizeFor(loaded.Candidates.Count);
        if (count == 0)
        {
            throw new AppException(
                CatalogErrors.InsufficientQuestions,
                details: new Dictionary<string, object?> { ["available"] = loaded.Candidates.Count });
        }

        // Random.Shared is fine here: tests check the sampler itself with a seeded Random.
        var questionIds = WeakAreaSampler.Sample(loaded.Candidates, count, loaded.Leaves, Random.Shared);

        // One transaction: if the attempt cannot start (e.g. EMAIL_NOT_VERIFIED), no set is left behind.
        await using var transaction = await db.BeginTransactionAsync(cancellationToken);
        var set = TestSet.CreateCustom(viewer.UserId, track, Settings(questionIds.Count), questionIds.Count, Criteria(track, found.Areas), clock.GetUtcNow());
        db.TestSets.Add(set);
        db.TestSetItems.AddRange(TestSetItem.ForQuestions(set.Id, questionIds));
        await db.SaveChangesAsync(cancellationToken);

        var attempt = (await startAttempt.HandleAsync(new StartAttemptRequest(set.Id), cancellationToken)).Attempt;
        await transaction.CommitAsync(cancellationToken);
        return new CreateCustomSetResponse(TestSetDetail.From(set, viewer), attempt);
    }

    // Practice with tutor reveal and no timer: the point is to learn from each rationale at once.
    private static TestSetSettings Settings(int count) =>
        new(
            Title: string.Create(CultureInfo.InvariantCulture, $"Weak areas practice: {count} questions"),
            Description: null,
            TestMode.Practice,
            RevealMode.Tutor,
            Timed: false,
            PerQuestionSec: null,
            TimeBudgetSec: null,
            PassThreshold: null,
            PausePolicy.Unlimited,
            AllowBackNav: true,
            IsSample: false,
            // The questions were already limited to the user's tier, so the set itself is open to its owner.
            PlanTier.Free);

    // The same shape as a builder set's criteria, plus where it came from, so "retake similar" can
    // treat both alike.
    private static JsonDocument Criteria(ExamTrack track, IReadOnlyList<WeakAreaView> areas) =>
        JsonSerializer.SerializeToDocument(
            new
            {
                track,
                source = "weak_areas",
                clientNeedsIds = areas.Select(a => a.ClientNeedsId).ToArray(),
                bodySystemIds = Array.Empty<short>(),
                itemTypes = Array.Empty<string>(),
                difficulties = Array.Empty<string>(),
                pool = PoolMode.All,
            },
            ItemJson.Options);
}
