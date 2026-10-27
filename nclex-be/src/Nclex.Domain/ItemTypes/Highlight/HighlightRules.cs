namespace Nclex.Domain.ItemTypes.Highlight;

/// <summary>
/// Grading and checks shared by highlight text and highlight table (interfaces.md 2.7). Both work on
/// a list of parts (segments or rows), some of which are selectable.
/// </summary>
public static class HighlightRules
{
    /// <summary>+/- over the selectable parts with a floor of 0; max points is the number of correct parts.</summary>
    public static GradeResult Grade(IReadOnlyList<string> selectableIds, HighlightKey key, HighlightResponse? response)
    {
        var (points, detail) = PlusMinusScoring.Score(selectableIds, key.Correct, response?.Selected ?? []);
        return GradeResult.From(points, key.Correct.Count, detail);
    }

    /// <summary>
    /// The key needs at least one correct part, and every correct part must be selectable. Every
    /// selectable part needs a rationale.
    /// </summary>
    public static IEnumerable<ValidationIssue> CheckKeyAndRationale(
        IReadOnlyList<(string Id, bool Selectable)> parts,
        HighlightKey key,
        Rationale rationale,
        string noun)
    {
        if (parts.Count(p => p.Selectable) < 2)
        {
            yield return new ValidationIssue("content", ValidationIssueCodes.TooFew, $"At least 2 {noun}s must be selectable.");
        }

        if (key.Correct.Count == 0)
        {
            yield return new ValidationIssue("key.correct", ValidationIssueCodes.TooFew, $"At least one {noun} must be correct.");
        }

        foreach (var issue in CheckSelection(parts, key.Correct, "key.correct", noun))
        {
            yield return issue;
        }

        foreach (var issue in IdRules.CheckRationale(rationale, SelectableIds(parts)))
        {
            yield return issue;
        }
    }

    /// <summary>
    /// Each id must be a part, selectable and listed once. A part that exists but is not selectable gets
    /// its own code, because the player should never have let the candidate pick it.
    /// </summary>
    public static IEnumerable<ValidationIssue> CheckSelection(
        IReadOnlyList<(string Id, bool Selectable)> parts,
        IReadOnlyList<string> ids,
        string path,
        string noun)
    {
        var selectable = parts.ToDictionary(p => p.Id, p => p.Selectable, StringComparer.Ordinal);
        var seen = new HashSet<string>(StringComparer.Ordinal);
        for (var i = 0; i < ids.Count; i++)
        {
            if (!selectable.TryGetValue(ids[i], out var canSelect))
            {
                yield return new ValidationIssue($"{path}[{i}]", ValidationIssueCodes.UnknownId, $"There is no {noun} \"{ids[i]}\".");
            }
            else if (!canSelect)
            {
                yield return new ValidationIssue($"{path}[{i}]", ValidationIssueCodes.NotSelectable, $"The {noun} \"{ids[i]}\" can't be selected.");
            }
            else if (!seen.Add(ids[i]))
            {
                yield return new ValidationIssue($"{path}[{i}]", ValidationIssueCodes.DuplicateId, $"The {noun} \"{ids[i]}\" is listed more than once.");
            }
        }
    }

    public static IReadOnlyList<string> SelectableIds(IReadOnlyList<(string Id, bool Selectable)> parts) =>
        [.. parts.Where(p => p.Selectable).Select(p => p.Id)];
}
