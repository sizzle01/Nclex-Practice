namespace Nclex.Domain.ItemTypes.Matrix;

/// <summary>
/// Matrix multiple choice: one choice per row, scored 0/1 per row (interfaces.md 2.4).
/// </summary>
public sealed class MatrixMcModule : ItemTypeModule<MatrixContent, MatrixMcKey, MatrixMcResponse>
{
    public override ItemType Type => ItemType.MatrixMc;

    public override Phase Phase => Phase.Mvp;

    public override decimal MaxPoints(MatrixContent content, MatrixMcKey key) => content.Rows.Count;

    public override GradeResult Grade(MatrixContent content, MatrixMcKey key, MatrixMcResponse? response)
    {
        var detail = content.Rows.Select(row =>
        {
            var expected = key.ByRow[row.Id];
            var chosen = response?.ByRow.GetValueOrDefault(row.Id);
            return chosen == expected
                ? new GradeDetail(row.Id, +1, Selected: chosen, Expected: expected)
                : new GradeDetail(row.Id, 0, Selected: chosen, Expected: expected, Missed: true);
        }).ToList();
        return GradeResult.From(detail.Sum(d => d.Delta), MaxPoints(content, key), detail);
    }

    public override IReadOnlyList<ValidationIssue> ValidateAuthoring(MatrixContent content, MatrixMcKey key, Rationale rationale)
    {
        var issues = MatrixRules.CheckGrid(content.Rows, content.Columns).ToList();
        var rowIds = content.Rows.Select(r => r.Id).ToHashSet(StringComparer.Ordinal);
        var columnIds = content.Columns.Select(c => c.Id).ToHashSet(StringComparer.Ordinal);

        foreach (var row in content.Rows.Where(r => !key.ByRow.ContainsKey(r.Id)))
        {
            issues.Add(new ValidationIssue($"key.byRow.{row.Id}", ValidationIssueCodes.Required, $"Row \"{row.Id}\" needs a correct column."));
        }

        issues.AddRange(CheckChoices(key.ByRow!, rowIds, columnIds, "key.byRow"));
        issues.AddRange(IdRules.CheckRationale(rationale, MatrixRules.CellIds(content.Rows, content.Columns)));
        return issues;
    }

    // Unanswered rows are fine on autosave: the candidate may not have reached them yet.
    public override IReadOnlyList<ValidationIssue> ValidateResponse(MatrixContent content, MatrixMcResponse response) =>
        [.. CheckChoices(
            response.ByRow,
            content.Rows.Select(r => r.Id).ToHashSet(StringComparer.Ordinal),
            content.Columns.Select(c => c.Id).ToHashSet(StringComparer.Ordinal),
            "response.byRow")];

    private static IEnumerable<ValidationIssue> CheckChoices(
        IReadOnlyDictionary<string, string?> byRow,
        HashSet<string> rowIds,
        HashSet<string> columnIds,
        string path)
    {
        foreach (var (rowId, columnId) in byRow)
        {
            if (!rowIds.Contains(rowId))
            {
                yield return new ValidationIssue($"{path}.{rowId}", ValidationIssueCodes.UnknownId, $"There is no row \"{rowId}\".");
            }
            else if (columnId is not null && !columnIds.Contains(columnId))
            {
                yield return new ValidationIssue($"{path}.{rowId}", ValidationIssueCodes.UnknownId, $"There is no column \"{columnId}\".");
            }
        }
    }
}
