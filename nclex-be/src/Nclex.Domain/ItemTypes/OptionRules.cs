namespace Nclex.Domain.ItemTypes;

/// <summary>
/// Checks shared by item types whose content is a list of options (multiple choice, SATA, select N...).
/// </summary>
public static class OptionRules
{
    /// <summary>
    /// Options must have unique, non-empty ids and non-empty text, and their number must be within
    /// <paramref name="min"/> and <paramref name="max"/>.
    /// </summary>
    public static IEnumerable<ValidationIssue> CheckOptions(IReadOnlyList<Option> options, int min, int max = int.MaxValue)
    {
        if (options.Count < min)
        {
            yield return new ValidationIssue("content.options", ValidationIssueCodes.TooFew, $"At least {min} options are needed.");
        }

        if (options.Count > max)
        {
            yield return new ValidationIssue("content.options", ValidationIssueCodes.TooMany, $"At most {max} options are allowed.");
        }

        var seen = new HashSet<string>(StringComparer.Ordinal);
        for (var i = 0; i < options.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(options[i].Id))
            {
                yield return new ValidationIssue($"content.options[{i}].id", ValidationIssueCodes.Required, "Every option needs an id.");
            }
            else if (!seen.Add(options[i].Id))
            {
                yield return new ValidationIssue($"content.options[{i}].id", ValidationIssueCodes.DuplicateId, $"Option id \"{options[i].Id}\" is used more than once.");
            }

            if (string.IsNullOrWhiteSpace(options[i].Text))
            {
                yield return new ValidationIssue($"content.options[{i}].text", ValidationIssueCodes.Required, "Every option needs text.");
            }
        }
    }

    /// <summary>
    /// The rationale must explain the correct answer and give a "why" for every option, and only for
    /// options that exist (database.md 2.4 rules; PRD 5.1).
    /// </summary>
    public static IEnumerable<ValidationIssue> CheckRationale(Rationale rationale, IReadOnlyList<Option> options)
    {
        if (string.IsNullOrWhiteSpace(rationale.Correct))
        {
            yield return new ValidationIssue("rationale.correct", ValidationIssueCodes.Required, "Explain why the correct answer is correct.");
        }

        var optionIds = options.Select(o => o.Id).ToHashSet(StringComparer.Ordinal);
        foreach (var option in options)
        {
            if (!rationale.PerOption.TryGetValue(option.Id, out var why) || string.IsNullOrWhiteSpace(why))
            {
                yield return new ValidationIssue($"rationale.perOption.{option.Id}", ValidationIssueCodes.MissingRationale, $"Option \"{option.Id}\" needs a rationale.");
            }
        }

        foreach (var id in rationale.PerOption.Keys.Where(id => !optionIds.Contains(id)))
        {
            yield return new ValidationIssue($"rationale.perOption.{id}", ValidationIssueCodes.UnknownId, $"There is no option \"{id}\".");
        }
    }

    /// <summary>Each id in <paramref name="ids"/> must be an option, and appear only once.</summary>
    public static IEnumerable<ValidationIssue> CheckSelection(IReadOnlyList<string> ids, IReadOnlyList<Option> options, string path)
    {
        var optionIds = options.Select(o => o.Id).ToHashSet(StringComparer.Ordinal);
        var seen = new HashSet<string>(StringComparer.Ordinal);
        for (var i = 0; i < ids.Count; i++)
        {
            if (!optionIds.Contains(ids[i]))
            {
                yield return new ValidationIssue($"{path}[{i}]", ValidationIssueCodes.UnknownId, $"There is no option \"{ids[i]}\".");
            }
            else if (!seen.Add(ids[i]))
            {
                yield return new ValidationIssue($"{path}[{i}]", ValidationIssueCodes.DuplicateId, $"Option \"{ids[i]}\" is listed more than once.");
            }
        }
    }
}
