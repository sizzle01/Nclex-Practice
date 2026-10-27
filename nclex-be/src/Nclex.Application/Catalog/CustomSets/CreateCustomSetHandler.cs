using System.Globalization;
using System.Text.Json;
using Nclex.Application.Abstractions;
using Nclex.Application.Common;
using Nclex.Domain.Builder;
using Nclex.Domain.Entities;
using Nclex.Domain.ItemTypes;

namespace Nclex.Application.Catalog.CustomSets;

/// <summary>
/// POST /test-sets/custom (architecture.md 5.7): builds the pool, samples the set and saves it as
/// the user's own custom set. Starting the attempt is added by F08.
/// </summary>
public sealed class CreateCustomSetHandler(
    IAppDbContext db,
    PlanTierGuard planTierGuard,
    QuestionPool questionPool,
    TimeProvider clock)
{
    /// <summary>
    /// Challenge sets estimate pass or fail, as the seeded Comprehensive Challenge does; the request
    /// has no field for it (api.md section 3).
    /// </summary>
    public const decimal ChallengePassThreshold = 0.65m;

    public async Task<CreateCustomSetResponse> HandleAsync(CreateCustomSetRequest request, CancellationToken cancellationToken)
    {
        var viewer = await planTierGuard.LoadViewerAsync(cancellationToken);
        var pool = await questionPool.LoadAsync(request, viewer, cancellationToken);
        var count = request.QuestionCount!.Value;

        // Random.Shared is fine here: tests check the sampler itself with a seeded Random.
        var questionIds = SetSampler.Sample(pool.Items, count, pool.SelectedLeaves, Random.Shared);
        if (questionIds.Count < count)
        {
            throw new AppException(
                CatalogErrors.InsufficientQuestions,
                details: new Dictionary<string, object?> { ["available"] = questionIds.Count });
        }

        var now = clock.GetUtcNow();
        var set = TestSet.CreateCustom(viewer.UserId, pool.Track, Settings(request), count, Criteria(request, pool.Track), now);
        db.TestSets.Add(set);
        db.TestSetItems.AddRange(TestSetItem.ForQuestions(set.Id, questionIds));
        await db.SaveChangesAsync(cancellationToken);

        return new CreateCustomSetResponse(TestSetDetail.From(set, viewer), Attempt: null);
    }

    private static TestSetSettings Settings(CreateCustomSetRequest request)
    {
        var timing = request.Timing ?? new CustomSetTiming();
        var mode = request.Mode!.Value;
        return new TestSetSettings(
            Title: string.Create(CultureInfo.InvariantCulture, $"Custom test: {request.QuestionCount} questions"),
            Description: null,
            mode,
            request.RevealMode!.Value,
            timing.Timed,
            timing.PerQuestionSec,
            timing.TimeBudgetSec,
            PassThreshold: mode == TestMode.Challenge ? ChallengePassThreshold : null,
            PausePolicy.Unlimited,
            AllowBackNav: true,
            IsSample: false,
            // The questions were already limited to the user's tier, so the set itself is open to its owner.
            PlanTier.Free);
    }

    // The filters as the user chose them (parents not expanded), for "retake similar" later.
    private static JsonDocument Criteria(CreateCustomSetRequest request, ExamTrack track) =>
        JsonSerializer.SerializeToDocument(
            new
            {
                track,
                clientNeedsIds = request.ClientNeedsIds ?? [],
                bodySystemIds = request.BodySystemIds ?? [],
                itemTypes = request.ItemTypes ?? [],
                difficulties = request.Difficulties ?? [],
                pool = request.Pool ?? PoolMode.All,
            },
            ItemJson.Options);
}
