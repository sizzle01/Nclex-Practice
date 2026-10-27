namespace Nclex.Domain.ItemTypes;

/// <summary>
/// Id checks shared by item types whose content is not a flat option list: rows, columns, blanks,
/// segments, groups and the ids that keys and responses use to refer to them.
/// </summary>
public static class IdRules
{
    /// <summary>
    /// Ids declared by the content must be non-empty and unique. <paramref name="noun"/> names them
    /// in messages, e.g. "row".
    /// </summary>
    public static IEnumerable<ValidationIssue> CheckDeclared(IReadOnlyList<string> ids, Func<int, string> pathOf, string noun)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        for (var i = 0; i < ids.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(ids[i]))
            {
                yield return new ValidationIssue(pathOf(i), ValidationIssueCodes.Required, $"Every {noun} needs an id.");
            }
            else if (!seen.Add(ids[i]))
            {
                yield return new ValidationIssue(pathOf(i), ValidationIssueCodes.DuplicateId, $"The {noun} id \"{ids[i]}\" is used more than once.");
            }
        }
    }

    /// <summary>
    /// Each id in a key or response must be one of <paramref name="known"/>, and appear only once.
    /// </summary>
    public static IEnumerable<ValidationIssue> CheckReferences(
        IReadOnlyList<string> ids,
        IReadOnlySet<string> known,
        string path,
        string noun)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        for (var i = 0; i < ids.Count; i++)
        {
            if (!known.Contains(ids[i]))
            {
                yield return new ValidationIssue($"{path}[{i}]", ValidationIssueCodes.UnknownId, $"There is no {noun} \"{ids[i]}\".");
            }
            else if (!seen.Add(ids[i]))
            {
                yield return new ValidationIssue($"{path}[{i}]", ValidationIssueCodes.DuplicateId, $"The {noun} \"{ids[i]}\" is listed more than once.");
            }
        }
    }

    /// <summary>
    /// The rationale must explain the correct answer and give a "why" for every selectable id, and only
    /// for ids that exist (interfaces.md section 2: "rationale.perOption keys must cover every selectable ID").
    /// </summary>
    public static IEnumerable<ValidationIssue> CheckRationale(Rationale rationale, IReadOnlyList<string> selectableIds)
    {
        if (string.IsNullOrWhiteSpace(rationale.Correct))
        {
            yield return new ValidationIssue("rationale.correct", ValidationIssueCodes.Required, "Explain why the correct answer is correct.");
        }

        foreach (var id in selectableIds)
        {
            if (!rationale.PerOption.TryGetValue(id, out var why) || string.IsNullOrWhiteSpace(why))
            {
                yield return new ValidationIssue($"rationale.perOption.{id}", ValidationIssueCodes.MissingRationale, $"\"{id}\" needs a rationale.");
            }
        }

        var known = selectableIds.ToHashSet(StringComparer.Ordinal);
        foreach (var id in rationale.PerOption.Keys.Where(id => !known.Contains(id)))
        {
            yield return new ValidationIssue($"rationale.perOption.{id}", ValidationIssueCodes.UnknownId, $"There is nothing selectable with id \"{id}\".");
        }
    }
}
