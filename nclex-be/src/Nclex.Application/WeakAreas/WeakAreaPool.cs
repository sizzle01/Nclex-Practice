using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Application.Catalog;
using Nclex.Application.Catalog.CustomSets;
using Nclex.Domain.Builder;
using Nclex.Domain.Entities;
using Nclex.Domain.ItemTypes;

namespace Nclex.Application.WeakAreas;

/// <summary>The questions a weak-area test may use, and the weak leaves with their test-plan weights.</summary>
public sealed record LoadedWeakAreaPool(IReadOnlyList<WeakAreaCandidate> Candidates, IReadOnlyList<LeafWeight> Leaves);

/// <summary>
/// The pool of a weak-area practice test (architecture.md 5.7.1). Eligibility (published, on the
/// track, within the user's plan) comes from the custom builder's <see cref="QuestionPool"/>, so the
/// two can never disagree; this class adds what the user last did with each question.
/// </summary>
public sealed class WeakAreaPool(IAppDbContext db, QuestionPool questionPool)
{
    public async Task<LoadedWeakAreaPool> LoadAsync(
        CatalogViewer viewer,
        ExamTrack track,
        IReadOnlyList<WeakAreaView> areas,
        CancellationToken cancellationToken)
    {
        if (areas.Count == 0)
        {
            return new LoadedWeakAreaPool([], []);
        }

        var pool = await questionPool.LoadAsync(new WeakAreaFilters(track, [.. areas.Select(a => a.ClientNeedsId)]), viewer, cancellationToken);

        // Case-study items are left out: a case study's six items span several leaves, so it can't
        // be targeted at one weak area, and it would take a quarter of a 25-question test.
        var singles = pool.Items.Where(i => i.CaseStudyId is null).ToList();
        var questionIds = singles.Select(i => i.QuestionId).ToList();
        var lastCorrect = await db.UserQuestionHistory.AsNoTracking()
            .Where(h => h.UserId == viewer.UserId && questionIds.Contains(h.QuestionId))
            .ToDictionaryAsync(h => h.QuestionId, h => h.LastCorrect, cancellationToken);

        return new LoadedWeakAreaPool(
            [.. singles.Select(i => new WeakAreaCandidate(i.QuestionId, i.ClientNeedsId, PastResultOf(i.QuestionId, lastCorrect)))],
            pool.SelectedLeaves);
    }

    // A question seen but left blank has LastCorrect false or null; either way it counts as missed.
    private static PastResult PastResultOf(Guid questionId, Dictionary<Guid, bool?> lastCorrect) =>
        !lastCorrect.TryGetValue(questionId, out var correct) ? PastResult.Unseen
        : correct == true ? PastResult.Correct
        : PastResult.Missed;

    // The builder's filters for "any question in these leaves".
    private sealed record WeakAreaFilters(ExamTrack Track, IReadOnlyList<short> Leaves) : IPoolFilters
    {
        ExamTrack? IPoolFilters.Track => Track;

        public IReadOnlyList<short>? ClientNeedsIds => Leaves;

        public IReadOnlyList<short>? BodySystemIds => null;

        public IReadOnlyList<ItemType>? ItemTypes => null;

        public IReadOnlyList<DifficultyTier>? Difficulties => null;

        public PoolMode? Pool => PoolMode.All;
    }
}
