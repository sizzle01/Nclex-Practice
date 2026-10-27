namespace Nclex.Domain.ItemTypes.Dropdown;

/// <summary>
/// Grading and checks shared by drop-down cloze and drop-down table (interfaces.md 2.6): each blank
/// scores 0 or 1, and rationales are keyed "blankId:optionId".
/// </summary>
public static class BlankGrader
{
    /// <summary>Joins a blank id and one of its option ids into a <c>rationale.perOption</c> key.</summary>
    public const char RationaleSeparator = ':';

    private const int MinOptionsPerBlank = 2;

    public static decimal MaxPoints(IReadOnlyList<Blank> blanks) => blanks.Count;

    public static GradeResult Grade(IReadOnlyList<Blank> blanks, DropdownKey key, DropdownResponse? response)
    {
        var detail = blanks.Select(blank =>
        {
            var expected = key.ByBlank[blank.Id];
            var chosen = response?.ByBlank.GetValueOrDefault(blank.Id);
            return chosen == expected
                ? new GradeDetail(blank.Id, +1, Selected: chosen, Expected: expected)
                : new GradeDetail(blank.Id, 0, Selected: chosen, Expected: expected, Missed: true);
        }).ToList();
        return GradeResult.From(detail.Sum(d => d.Delta), MaxPoints(blanks), detail);
    }

    /// <summary>
    /// At least one blank; unique blank ids without the rationale separator, so "blankId:optionId"
    /// keys can't collide; and at least two options with unique ids and text in each blank.
    /// </summary>
    public static IEnumerable<ValidationIssue> CheckBlanks(IReadOnlyList<Blank> blanks)
    {
        if (blanks.Count < 1)
        {
            yield return new ValidationIssue("content.blanks", ValidationIssueCodes.TooFew, "At least 1 blank is needed.");
        }

        foreach (var issue in IdRules.CheckDeclared([.. blanks.Select(b => b.Id)], i => $"content.blanks[{i}].id", "blank"))
        {
            yield return issue;
        }

        for (var b = 0; b < blanks.Count; b++)
        {
            var path = $"content.blanks[{b}]";
            if (blanks[b].Id.Contains(RationaleSeparator, StringComparison.Ordinal))
            {
                yield return new ValidationIssue($"{path}.id", ValidationIssueCodes.InvalidValue, $"A blank id can't contain \"{RationaleSeparator}\".");
            }

            foreach (var issue in OptionRules.CheckOptions(blanks[b].Options, MinOptionsPerBlank, path: $"{path}.options"))
            {
                yield return issue;
            }
        }
    }

    /// <summary>
    /// Each blank must appear exactly once in the template or table, and nothing may refer to a blank
    /// that does not exist.
    /// </summary>
    /// <param name="blanks">The declared blanks.</param>
    /// <param name="placements">Each place a blank id is used, with the path to report it at.</param>
    public static IEnumerable<ValidationIssue> CheckPlacements(IReadOnlyList<Blank> blanks, IReadOnlyList<(string BlankId, string Path)> placements)
    {
        var declared = blanks.Select(b => b.Id).ToHashSet(StringComparer.Ordinal);
        var placed = new HashSet<string>(StringComparer.Ordinal);
        foreach (var (blankId, path) in placements)
        {
            if (!declared.Contains(blankId))
            {
                yield return new ValidationIssue(path, ValidationIssueCodes.UnknownId, $"There is no blank \"{blankId}\".");
            }
            else if (!placed.Add(blankId))
            {
                yield return new ValidationIssue(path, ValidationIssueCodes.DuplicateId, $"Blank \"{blankId}\" is used more than once.");
            }
        }

        for (var b = 0; b < blanks.Count; b++)
        {
            if (!placed.Contains(blanks[b].Id))
            {
                yield return new ValidationIssue($"content.blanks[{b}]", ValidationIssueCodes.Mismatch, $"Blank \"{blanks[b].Id}\" is never shown.");
            }
        }
    }

    /// <summary>Every blank needs a correct option, and it must be one of that blank's options.</summary>
    public static IEnumerable<ValidationIssue> CheckKey(IReadOnlyList<Blank> blanks, DropdownKey key)
    {
        foreach (var blank in blanks.Where(b => !key.ByBlank.ContainsKey(b.Id)))
        {
            yield return new ValidationIssue($"key.byBlank.{blank.Id}", ValidationIssueCodes.Required, $"Blank \"{blank.Id}\" needs a correct option.");
        }

        foreach (var issue in CheckChoices(blanks, key.ByBlank!, "key.byBlank"))
        {
            yield return issue;
        }
    }

    /// <summary>Unanswered blanks are fine on autosave; answered ones must name one of that blank's options.</summary>
    public static IEnumerable<ValidationIssue> CheckResponse(IReadOnlyList<Blank> blanks, DropdownResponse response) =>
        CheckChoices(blanks, response.ByBlank, "response.byBlank");

    /// <summary>The <c>rationale.perOption</c> keys an item needs: one per option of every blank.</summary>
    public static IReadOnlyList<string> RationaleIds(IReadOnlyList<Blank> blanks) =>
        [.. from blank in blanks from option in blank.Options select $"{blank.Id}{RationaleSeparator}{option.Id}"];

    private static IEnumerable<ValidationIssue> CheckChoices(IReadOnlyList<Blank> blanks, IReadOnlyDictionary<string, string?> byBlank, string path)
    {
        var blanksById = blanks.ToDictionary(b => b.Id, StringComparer.Ordinal);
        foreach (var (blankId, optionId) in byBlank)
        {
            if (!blanksById.TryGetValue(blankId, out var blank))
            {
                yield return new ValidationIssue($"{path}.{blankId}", ValidationIssueCodes.UnknownId, $"There is no blank \"{blankId}\".");
            }
            else if (optionId is not null && !blank.Options.Any(o => o.Id == optionId))
            {
                yield return new ValidationIssue($"{path}.{blankId}", ValidationIssueCodes.UnknownId, $"Blank \"{blankId}\" has no option \"{optionId}\".");
            }
        }
    }
}
