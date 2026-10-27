namespace Nclex.Domain.Builder;

/// <summary>
/// Splits a set's question count across the selected Client Needs leaves in proportion to their
/// test-plan weights (architecture.md 5.7), so a 30-question set on "Management of Care" and
/// "Pharmacological Therapies" looks like the real exam's mix.
/// </summary>
public static class TestPlanAllocation
{
    /// <summary>
    /// How many questions to take from each leaf. Shares are rounded with the largest-remainder
    /// method, so they add up to <paramref name="count"/>, and then capped at what each leaf has
    /// available; the builder fills what the caps leave out at random.
    /// </summary>
    /// <param name="count">Questions wanted in total.</param>
    /// <param name="leaves">The selected leaves and their weights.</param>
    /// <param name="available">Questions each leaf can supply; a missing leaf has none.</param>
    public static IReadOnlyDictionary<short, int> Allocate(
        int count,
        IReadOnlyList<LeafWeight> leaves,
        IReadOnlyDictionary<short, int> available)
    {
        var totalWeight = leaves.Sum(l => l.Weight);
        if (leaves.Count == 0 || totalWeight <= 0)
        {
            return new Dictionary<short, int>();
        }

        var exact = leaves.Select(l => (l.ClientNeedsId, Share: count * l.Weight / totalWeight)).ToList();
        var quotas = exact.ToDictionary(e => e.ClientNeedsId, e => (int)Math.Floor(e.Share));
        var leftOver = count - quotas.Values.Sum();

        // Largest fractional part first; ties go to the lower id so the result never depends on input order.
        foreach (var (id, _) in exact.OrderByDescending(e => e.Share - Math.Floor(e.Share)).ThenBy(e => e.ClientNeedsId).Take(leftOver))
        {
            quotas[id]++;
        }

        return quotas.ToDictionary(q => q.Key, q => Math.Min(q.Value, available.GetValueOrDefault(q.Key)));
    }
}
