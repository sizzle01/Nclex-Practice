namespace Nclex.Domain.ItemTypes;

/// <summary>
/// The NCSBN +/- rule used by highlight items and by each group of matrix multiple response and
/// multiple response grouping items (interfaces.md 2.5 and 2.7): a correct selection earns a point,
/// an incorrect one loses a point, and the total can't go below zero.
/// </summary>
public static class PlusMinusScoring
{
    /// <summary>Scores one set of choices.</summary>
    /// <param name="choices">Every selectable id, in display order; the detail follows this order.</param>
    /// <param name="correct">The ids that should be selected.</param>
    /// <param name="selected">The ids the candidate selected.</param>
    public static (int Points, IReadOnlyList<GradeDetail> Detail) Score(
        IEnumerable<string> choices,
        IReadOnlyCollection<string> correct,
        IReadOnlyCollection<string> selected)
    {
        var correctSet = correct.ToHashSet(StringComparer.Ordinal);
        var selectedSet = selected.ToHashSet(StringComparer.Ordinal);
        var detail = choices.Select(id => (chosen: selectedSet.Contains(id), ok: correctSet.Contains(id)) switch
        {
            (true, true) => new GradeDetail(id, +1, Selected: id),
            (true, false) => new GradeDetail(id, -1, Selected: id),
            (false, true) => new GradeDetail(id, 0, Missed: true),
            _ => new GradeDetail(id, 0),
        }).ToList();
        return (Math.Max(0, detail.Sum(d => d.Delta)), detail);
    }
}
