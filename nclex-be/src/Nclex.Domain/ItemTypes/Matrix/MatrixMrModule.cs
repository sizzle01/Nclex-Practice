using Nclex.Domain.ItemTypes.Grouped;

namespace Nclex.Domain.ItemTypes.Matrix;

/// <summary>
/// Matrix multiple response: candidates tick any number of cells, and each row (or each column, by
/// <see cref="MatrixMrContent.GroupAxis"/>) is scored +/- with a floor of 0 (interfaces.md 2.5).
/// Shares <see cref="GroupedGrader"/> with multiple response grouping.
/// </summary>
public sealed class MatrixMrModule : ItemTypeModule<MatrixMrContent, GroupedKey, GroupedResponse>
{
    public override ItemType Type => ItemType.MatrixMr;

    public override Phase Phase => Phase.Mvp;

    public override decimal MaxPoints(MatrixMrContent content, GroupedKey key) => GroupedGrader.MaxPoints(key);

    public override GradeResult Grade(MatrixMrContent content, GroupedKey key, GroupedResponse? response) =>
        GroupedGrader.Grade(Groups(content), key, response);

    public override IReadOnlyList<ValidationIssue> ValidateAuthoring(MatrixMrContent content, GroupedKey key, Rationale rationale)
    {
        var issues = MatrixRules.CheckGrid(content.Rows, content.Columns).ToList();
        issues.AddRange(GroupedGrader.CheckKey(Groups(content), key, "cell"));
        issues.AddRange(IdRules.CheckRationale(rationale, MatrixRules.CellIds(content.Rows, content.Columns)));
        return issues;
    }

    public override IReadOnlyList<ValidationIssue> ValidateResponse(MatrixMrContent content, GroupedResponse response) =>
        [.. GroupedGrader.CheckResponse(Groups(content), response, "cell")];

    // A row group holds that row's cells across every column; a column group holds that column's cells down every row.
    private static List<ChoiceGroup> Groups(MatrixMrContent content) => content.GroupAxis switch
    {
        MatrixGroupAxis.Row => [.. content.Rows.Select(row => new ChoiceGroup(
            row.Id, [.. content.Columns.Select(column => MatrixRules.CellId(row.Id, column.Id))]))],
        _ => [.. content.Columns.Select(column => new ChoiceGroup(
            column.Id, [.. content.Rows.Select(row => MatrixRules.CellId(row.Id, column.Id))]))],
    };
}
