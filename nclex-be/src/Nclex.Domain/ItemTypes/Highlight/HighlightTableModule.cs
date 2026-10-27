namespace Nclex.Domain.ItemTypes.Highlight;

/// <summary>
/// Highlight table: candidates highlight the rows of a table that answer the question, scored +/-
/// with a floor of 0 (interfaces.md 2.7).
/// </summary>
public sealed class HighlightTableModule : ItemTypeModule<HighlightTableContent, HighlightKey, HighlightResponse>
{
    private const string Noun = "row";

    public override ItemType Type => ItemType.HighlightTable;

    public override Phase Phase => Phase.Mvp;

    public override decimal MaxPoints(HighlightTableContent content, HighlightKey key) => key.Correct.Count;

    public override GradeResult Grade(HighlightTableContent content, HighlightKey key, HighlightResponse? response) =>
        HighlightRules.Grade(HighlightRules.SelectableIds(Parts(content)), key, response);

    public override IReadOnlyList<ValidationIssue> ValidateAuthoring(HighlightTableContent content, HighlightKey key, Rationale rationale)
    {
        var issues = new List<ValidationIssue>();
        if (content.Columns.Count < 1)
        {
            issues.Add(new ValidationIssue("content.columns", ValidationIssueCodes.TooFew, "At least 1 column is needed."));
        }

        issues.AddRange(IdRules.CheckDeclared([.. content.Rows.Select(r => r.Id)], i => $"content.rows[{i}].id", Noun));
        for (var i = 0; i < content.Rows.Count; i++)
        {
            if (content.Rows[i].Cells.Count != content.Columns.Count)
            {
                issues.Add(new ValidationIssue($"content.rows[{i}].cells", ValidationIssueCodes.Mismatch, $"Each row needs {content.Columns.Count} cells, one per column."));
            }
        }

        issues.AddRange(HighlightRules.CheckKeyAndRationale(Parts(content), key, rationale, Noun));
        return issues;
    }

    public override IReadOnlyList<ValidationIssue> ValidateResponse(HighlightTableContent content, HighlightResponse response) =>
        [.. HighlightRules.CheckSelection(Parts(content), response.Selected, "response.selected", Noun)];

    private static List<(string Id, bool Selectable)> Parts(HighlightTableContent content) =>
        [.. content.Rows.Select(r => (r.Id, r.Selectable))];
}
