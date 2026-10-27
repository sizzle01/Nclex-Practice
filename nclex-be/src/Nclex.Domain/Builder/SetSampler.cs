namespace Nclex.Domain.Builder;

/// <summary>
/// Picks the questions of a custom set from the pool (architecture.md 5.7):
/// <list type="number">
/// <item>with two or more leaves selected, each leaf gets its test-plan share (<see cref="TestPlanAllocation"/>);</item>
/// <item>whatever is still missing is filled at random from the rest of the pool;</item>
/// <item>a case study is one block: all of its items, in case order, or none of them.</item>
/// </list>
/// Case studies only join in step 2, because their items can span several leaves.
/// </summary>
public static class SetSampler
{
    /// <summary>
    /// Returns the question ids in serving order: up to <paramref name="count"/> of them, fewer
    /// only when the pool can't make up that many (the caller reports INSUFFICIENT_QUESTIONS).
    /// </summary>
    /// <param name="pool">Eligible questions; a case study must be complete.</param>
    /// <param name="count">Questions wanted.</param>
    /// <param name="leafWeights">The selected leaves with weights; proportional sampling needs two or more.</param>
    /// <param name="random">Seed it in tests to get the same set every time.</param>
    public static IReadOnlyList<Guid> Sample(
        IReadOnlyList<PoolItem> pool,
        int count,
        IReadOnlyList<LeafWeight> leafWeights,
        Random random)
    {
        // Sorted first, so a seeded Random gives the same result whatever order the database used.
        var units = ToUnits(pool.OrderBy(i => i.QuestionId));
        var chosen = new List<Unit>();
        var remaining = count;

        if (leafWeights.Count >= 2)
        {
            var singlesByLeaf = units.Where(u => u.IsSingle).GroupBy(u => u.Items[0].ClientNeedsId)
                .ToDictionary(g => g.Key, g => g.ToArray());
            var quotas = TestPlanAllocation.Allocate(count, leafWeights, singlesByLeaf.ToDictionary(g => g.Key, g => g.Value.Length));
            foreach (var (leaf, quota) in quotas.OrderBy(q => q.Key))
            {
                var singles = singlesByLeaf.GetValueOrDefault(leaf, []);
                random.Shuffle(singles);
                chosen.AddRange(singles.Take(quota));
            }

            remaining -= chosen.Count;
        }

        var rest = units.Except(chosen).ToArray();
        random.Shuffle(rest);
        foreach (var unit in rest)
        {
            if (remaining == 0)
            {
                break;
            }

            if (unit.Items.Count <= remaining)
            {
                chosen.Add(unit);
                remaining -= unit.Items.Count;
            }
        }

        // Mix the categories up, as on the real exam, but keep each case study's items together.
        var order = chosen.ToArray();
        random.Shuffle(order);
        return [.. order.SelectMany(u => u.Items).Select(i => i.QuestionId)];
    }

    private static List<Unit> ToUnits(IEnumerable<PoolItem> pool)
    {
        var units = new List<Unit>();
        foreach (var group in pool.GroupBy(i => i.CaseStudyId))
        {
            if (group.Key is null)
            {
                units.AddRange(group.Select(i => new Unit([i])));
            }
            else
            {
                units.Add(new Unit([.. group.OrderBy(i => i.CasePosition)]));
            }
        }

        return units;
    }

    // One single question, or one whole case study.
    private sealed record Unit(IReadOnlyList<PoolItem> Items)
    {
        public bool IsSingle => Items.Count == 1 && Items[0].CaseStudyId is null;
    }
}
