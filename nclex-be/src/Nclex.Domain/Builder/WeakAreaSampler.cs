namespace Nclex.Domain.Builder;

/// <summary>What the candidate last did with a question, which decides how soon it is picked.</summary>
public enum PastResult
{
    /// <summary>Their latest answer did not earn full marks, or they left it blank.</summary>
    Missed,

    /// <summary>They have never been served it.</summary>
    Unseen,

    /// <summary>Their latest answer earned full marks; used only when the others run out.</summary>
    Correct,
}

/// <summary>A single question (never part of a case study) in one of the candidate's weak areas.</summary>
/// <param name="QuestionId">The question.</param>
/// <param name="ClientNeedsId">The weak leaf it is tagged to.</param>
/// <param name="PastResult">What the candidate last did with it.</param>
public sealed record WeakAreaCandidate(Guid QuestionId, short ClientNeedsId, PastResult PastResult);

/// <summary>
/// Picks the questions of a weak-area practice test (architecture.md 5.7.1):
/// <list type="number">
/// <item>each weak leaf gets its test-plan share of the count (<see cref="TestPlanAllocation"/>);</item>
/// <item>within a leaf, missed questions come first, then unseen ones, then correct ones;</item>
/// <item>a leaf that runs short leaves its places to the other leaves, in the same order of preference.</item>
/// </list>
/// </summary>
public static class WeakAreaSampler
{
    /// <summary>The question ids in serving order: <paramref name="count"/> of them, or the whole pool when it is smaller.</summary>
    /// <param name="pool">The candidates in the weak leaves.</param>
    /// <param name="count">Questions wanted.</param>
    /// <param name="leafWeights">The weak leaves with their test-plan weights.</param>
    /// <param name="random">Seed it in tests to get the same set every time.</param>
    public static IReadOnlyList<Guid> Sample(
        IReadOnlyList<WeakAreaCandidate> pool,
        int count,
        IReadOnlyList<LeafWeight> leafWeights,
        Random random)
    {
        // Sorted first, so a seeded Random gives the same result whatever order the database used.
        var byLeaf = pool.OrderBy(c => c.QuestionId)
            .GroupBy(c => c.ClientNeedsId)
            .ToDictionary(g => g.Key, g => InPreferenceOrder([.. g], random));

        var quotas = TestPlanAllocation.Allocate(count, leafWeights, byLeaf.ToDictionary(g => g.Key, g => g.Value.Count));
        var chosen = new List<WeakAreaCandidate>();
        foreach (var (leaf, quota) in quotas.OrderBy(q => q.Key))
        {
            chosen.AddRange(byLeaf.GetValueOrDefault(leaf, []).Take(quota));
        }

        // Places a short leaf could not fill go to the best of what is left, missed questions first.
        var leftOver = InPreferenceOrder([.. byLeaf.Values.SelectMany(c => c).Except(chosen)], random);
        chosen.AddRange(leftOver.Take(count - chosen.Count));

        // Mix the areas up, as on the real exam, rather than serving all the missed ones first.
        var order = chosen.ToArray();
        random.Shuffle(order);
        return [.. order.Select(c => c.QuestionId)];
    }

    // Missed, then unseen, then correct; random within each group so a retake isn't the same test.
    private static List<WeakAreaCandidate> InPreferenceOrder(WeakAreaCandidate[] candidates, Random random)
    {
        random.Shuffle(candidates);
        return [.. candidates.OrderBy(c => c.PastResult)];
    }
}
