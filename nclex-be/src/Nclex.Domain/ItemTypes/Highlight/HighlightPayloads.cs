namespace Nclex.Domain.ItemTypes.Highlight;

/// <summary>
/// What candidates see in a highlight text item: a passage already split into segments by the
/// authoring tool; only selectable segments can be clicked (interfaces.md 2.7).
/// </summary>
public sealed record HighlightTextContent(IReadOnlyList<TextSegment> Segments);

/// <summary>One piece of a highlight passage.</summary>
public sealed record TextSegment(string Id, string Text, bool Selectable);

/// <summary>What candidates see in a highlight table item: a table whose selectable rows can be highlighted.</summary>
public sealed record HighlightTableContent(IReadOnlyList<string> Columns, IReadOnlyList<HighlightTableRow> Rows);

/// <summary>One row of a highlight table; it has one cell per column.</summary>
public sealed record HighlightTableRow(string Id, IReadOnlyList<string> Cells, bool Selectable);

/// <summary>The segment or row ids that should be highlighted.</summary>
public sealed record HighlightKey(IReadOnlyList<string> Correct);

/// <summary>The segment or row ids the candidate highlighted.</summary>
public sealed record HighlightResponse(IReadOnlyList<string> Selected);
