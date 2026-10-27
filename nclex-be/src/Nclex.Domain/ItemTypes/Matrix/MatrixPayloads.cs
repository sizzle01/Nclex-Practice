namespace Nclex.Domain.ItemTypes.Matrix;

/// <summary>A row or column heading of a matrix item.</summary>
public sealed record MatrixHeader(string Id, string Text);

/// <summary>What candidates see in a matrix multiple choice item: a grid of rows and columns (interfaces.md 2.4).</summary>
public sealed record MatrixContent(IReadOnlyList<MatrixHeader> Rows, IReadOnlyList<MatrixHeader> Columns);

/// <summary>The one correct column of every row, as row id to column id.</summary>
public sealed record MatrixMcKey(IReadOnlyDictionary<string, string> ByRow);

/// <summary>The column the candidate chose in each row; null or absent when the row is unanswered.</summary>
public sealed record MatrixMcResponse(IReadOnlyDictionary<string, string?> ByRow);

/// <summary>
/// What candidates see in a matrix multiple response item: a grid where each row (or each column)
/// is scored as its own group (interfaces.md 2.5).
/// </summary>
public sealed record MatrixMrContent(IReadOnlyList<MatrixHeader> Rows, IReadOnlyList<MatrixHeader> Columns, MatrixGroupAxis GroupAxis);

/// <summary>Whether the groups of a matrix multiple response item are its rows or its columns.</summary>
public enum MatrixGroupAxis
{
    Row,
    Column,
}
