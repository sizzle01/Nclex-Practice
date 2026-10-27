namespace Nclex.Domain.ItemTypes.Dropdown;

/// <summary>A drop-down list in a sentence or table cell, with its options (interfaces.md 2.6).</summary>
public sealed record Blank(string Id, IReadOnlyList<Option> Options);

/// <summary>
/// What candidates see in a drop-down cloze item: text with placeholders such as <c>{{b1}}</c>, each
/// replaced by the blank with that id.
/// </summary>
public sealed record DropdownClozeContent(string Template, IReadOnlyList<Blank> Blanks);

/// <summary>What candidates see in a drop-down table item: a table whose cells are text or blanks.</summary>
public sealed record DropdownTableContent(IReadOnlyList<string> Columns, IReadOnlyList<DropdownTableRow> Rows, IReadOnlyList<Blank> Blanks);

/// <summary>One row of a drop-down table; it has one cell per column.</summary>
public sealed record DropdownTableRow(string Id, IReadOnlyList<DropdownTableCell> Cells);

/// <summary>A table cell: either fixed text or a blank, never both.</summary>
public sealed record DropdownTableCell(string? Text = null, string? BlankId = null);

/// <summary>The correct option of every blank, as blank id to option id.</summary>
public sealed record DropdownKey(IReadOnlyDictionary<string, string> ByBlank);

/// <summary>The option the candidate chose in each blank; null or absent when the blank is unanswered.</summary>
public sealed record DropdownResponse(IReadOnlyDictionary<string, string?> ByBlank);
