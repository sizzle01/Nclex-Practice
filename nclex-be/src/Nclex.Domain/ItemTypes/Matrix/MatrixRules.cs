namespace Nclex.Domain.ItemTypes.Matrix;

/// <summary>Grid checks and cell ids shared by the two matrix item types.</summary>
public static class MatrixRules
{
    /// <summary>Joins row and column ids into the id of a cell (interfaces.md 2.5).</summary>
    public const char CellSeparator = ':';

    public static string CellId(string rowId, string columnId) => $"{rowId}{CellSeparator}{columnId}";

    /// <summary>Every cell id, row by row.</summary>
    public static IReadOnlyList<string> CellIds(IReadOnlyList<MatrixHeader> rows, IReadOnlyList<MatrixHeader> columns) =>
        [.. from row in rows from column in columns select CellId(row.Id, column.Id)];

    /// <summary>
    /// The grid needs at least one row and two columns, with unique ids and non-empty headings. Ids
    /// can't contain the cell separator, or two different cells could end up with the same id.
    /// </summary>
    public static IEnumerable<ValidationIssue> CheckGrid(IReadOnlyList<MatrixHeader> rows, IReadOnlyList<MatrixHeader> columns)
    {
        if (rows.Count < 1)
        {
            yield return new ValidationIssue("content.rows", ValidationIssueCodes.TooFew, "At least 1 row is needed.");
        }

        if (columns.Count < 2)
        {
            yield return new ValidationIssue("content.columns", ValidationIssueCodes.TooFew, "At least 2 columns are needed.");
        }

        foreach (var issue in CheckHeaders(rows, "rows", "row").Concat(CheckHeaders(columns, "columns", "column")))
        {
            yield return issue;
        }
    }

    private static IEnumerable<ValidationIssue> CheckHeaders(IReadOnlyList<MatrixHeader> headers, string field, string noun)
    {
        foreach (var issue in IdRules.CheckDeclared([.. headers.Select(h => h.Id)], i => $"content.{field}[{i}].id", noun))
        {
            yield return issue;
        }

        for (var i = 0; i < headers.Count; i++)
        {
            if (headers[i].Id.Contains(CellSeparator, StringComparison.Ordinal))
            {
                yield return new ValidationIssue($"content.{field}[{i}].id", ValidationIssueCodes.InvalidValue, $"A {noun} id can't contain \"{CellSeparator}\".");
            }

            if (string.IsNullOrWhiteSpace(headers[i].Text))
            {
                yield return new ValidationIssue($"content.{field}[{i}].text", ValidationIssueCodes.Required, $"Every {noun} needs text.");
            }
        }
    }
}
