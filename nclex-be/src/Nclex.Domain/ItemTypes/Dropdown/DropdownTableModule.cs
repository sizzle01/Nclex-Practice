namespace Nclex.Domain.ItemTypes.Dropdown;

/// <summary>
/// Drop-down table: a table whose cells are text or drop-down blanks, scored 0/1 per blank
/// (interfaces.md 2.6). Shares <see cref="BlankGrader"/> with drop-down cloze.
/// </summary>
public sealed class DropdownTableModule : ItemTypeModule<DropdownTableContent, DropdownKey, DropdownResponse>
{
    public override ItemType Type => ItemType.DropdownTable;

    public override Phase Phase => Phase.Mvp;

    public override decimal MaxPoints(DropdownTableContent content, DropdownKey key) => BlankGrader.MaxPoints(content.Blanks);

    public override GradeResult Grade(DropdownTableContent content, DropdownKey key, DropdownResponse? response) =>
        BlankGrader.Grade(content.Blanks, key, response);

    public override IReadOnlyList<ValidationIssue> ValidateAuthoring(DropdownTableContent content, DropdownKey key, Rationale rationale)
    {
        var issues = CheckTable(content).ToList();
        issues.AddRange(BlankGrader.CheckBlanks(content.Blanks));
        issues.AddRange(BlankGrader.CheckPlacements(content.Blanks, Placements(content)));
        issues.AddRange(BlankGrader.CheckKey(content.Blanks, key));
        issues.AddRange(IdRules.CheckRationale(rationale, BlankGrader.RationaleIds(content.Blanks)));
        return issues;
    }

    public override IReadOnlyList<ValidationIssue> ValidateResponse(DropdownTableContent content, DropdownResponse response) =>
        [.. BlankGrader.CheckResponse(content.Blanks, response)];

    private static IEnumerable<ValidationIssue> CheckTable(DropdownTableContent content)
    {
        if (content.Columns.Count < 1)
        {
            yield return new ValidationIssue("content.columns", ValidationIssueCodes.TooFew, "At least 1 column is needed.");
        }

        if (content.Rows.Count < 1)
        {
            yield return new ValidationIssue("content.rows", ValidationIssueCodes.TooFew, "At least 1 row is needed.");
        }

        foreach (var issue in IdRules.CheckDeclared([.. content.Rows.Select(r => r.Id)], i => $"content.rows[{i}].id", "row"))
        {
            yield return issue;
        }

        for (var r = 0; r < content.Rows.Count; r++)
        {
            var cells = content.Rows[r].Cells;
            if (cells.Count != content.Columns.Count)
            {
                yield return new ValidationIssue($"content.rows[{r}].cells", ValidationIssueCodes.Mismatch, $"Each row needs {content.Columns.Count} cells, one per column.");
            }

            for (var c = 0; c < cells.Count; c++)
            {
                if ((cells[c].Text is null) == (cells[c].BlankId is null))
                {
                    yield return new ValidationIssue($"content.rows[{r}].cells[{c}]", ValidationIssueCodes.InvalidValue, "A cell needs either text or a blankId, not both.");
                }
            }
        }
    }

    private static List<(string BlankId, string Path)> Placements(DropdownTableContent content) =>
        [.. content.Rows.SelectMany((row, r) => row.Cells
            .Select((cell, c) => (cell.BlankId, Path: $"content.rows[{r}].cells[{c}].blankId"))
            .Where(p => p.BlankId is not null)
            .Select(p => (p.BlankId!, p.Path)))];
}
