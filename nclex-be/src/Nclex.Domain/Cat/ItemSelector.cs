using Nclex.Domain.Builder;

namespace Nclex.Domain.Cat;

/// <summary>A question the adaptive exam may serve next.</summary>
/// <param name="QuestionId">The question (not a version).</param>
/// <param name="QuestionVersionId">Its published version, which the attempt item will pin.</param>
/// <param name="Difficulty">Its difficulty b, in logits.</param>
/// <param name="ClientNeedsId">The Client Needs leaf it is tagged to.</param>
public sealed record CatCandidate(Guid QuestionId, Guid QuestionVersionId, double Difficulty, short ClientNeedsId);

/// <summary>
/// Picks the next question (architecture.md 5.8): first the Client Needs leaf furthest behind its
/// test-plan share, then, within it, one of the most informative questions at the current θ.
/// </summary>
public static class ItemSelector
{
    /// <summary>
    /// How many of the most informative questions the pick is made from. Choosing at random among
    /// a few ("randomesque") stops every candidate at the same θ seeing the same questions.
    /// </summary>
    public const int RandomesqueSize = 5;

    /// <summary>
    /// The next question, or null when <paramref name="pool"/> is empty. The caller leaves out the
    /// questions already given in this attempt.
    /// </summary>
    /// <param name="contentCounts">Questions already given in this attempt, per Client Needs leaf.</param>
    /// <param name="testPlan">The track's leaves and their test-plan weights.</param>
    public static CatCandidate? Choose(
        IReadOnlyList<CatCandidate> pool,
        double theta,
        IReadOnlyDictionary<short, int> contentCounts,
        IReadOnlyList<LeafWeight> testPlan,
        Random random)
    {
        if (pool.Count == 0)
        {
            return null;
        }

        var area = MostBehindArea(pool, contentCounts, testPlan);
        var inArea = area is { } leaf ? pool.Where(c => c.ClientNeedsId == leaf).ToList() : [.. pool];

        // Shuffle first so questions of equal information (seeded difficulties are only −1, 0 and +1)
        // are not always taken in the same order; the sort that follows is stable.
        Shuffle(inArea, random);
        var best = inArea
            .OrderByDescending(c => RaschModel.Information(theta, c.Difficulty))
            .Take(RandomesqueSize)
            .ToList();
        return best[random.Next(best.Count)];
    }

    /// <summary>
    /// The leaf with the largest gap between its share of the next question and what it has had so
    /// far, among the leaves with questions left. Ties go to the lower id. Null when no leaf of the
    /// plan has questions, so the whole pool is used.
    /// </summary>
    private static short? MostBehindArea(
        IReadOnlyList<CatCandidate> pool,
        IReadOnlyDictionary<short, int> contentCounts,
        IReadOnlyList<LeafWeight> testPlan)
    {
        var withQuestions = pool.Select(c => c.ClientNeedsId).ToHashSet();
        var leaves = testPlan.Where(l => l.Weight > 0 && withQuestions.Contains(l.ClientNeedsId)).ToList();
        if (leaves.Count == 0)
        {
            return null;
        }

        var totalWeight = leaves.Sum(l => l.Weight);
        var given = contentCounts.Values.Sum();
        return leaves
            .Select(l => (l.ClientNeedsId, Gap: (l.Weight / totalWeight * (given + 1)) - contentCounts.GetValueOrDefault(l.ClientNeedsId)))
            .OrderByDescending(l => l.Gap)
            .ThenBy(l => l.ClientNeedsId)
            .First()
            .ClientNeedsId;
    }

    private static void Shuffle<T>(List<T> items, Random random)
    {
        for (var i = items.Count - 1; i > 0; i--)
        {
            var j = random.Next(i + 1);
            (items[i], items[j]) = (items[j], items[i]);
        }
    }
}
