namespace Nclex.Domain.ItemTypes;

/// <summary>Display name and phase of an item type, whether or not its module is built yet.</summary>
public sealed record ItemTypeDescription(ItemType Type, string Label, Phase Phase);

/// <summary>
/// Every item type with its label and phase, in the order of interfaces.md section 2.
/// GET /taxonomy lists them all and marks which ones have a module (api.md section 3).
/// </summary>
public static class ItemTypeCatalog
{
    public static IReadOnlyList<ItemTypeDescription> All { get; } =
    [
        new(ItemType.MultipleChoice, "Multiple Choice", Phase.Mvp),
        new(ItemType.Sata, "Select All That Apply", Phase.Mvp),
        new(ItemType.SelectN, "Select N", Phase.Mvp),
        new(ItemType.MatrixMc, "Matrix Multiple Choice", Phase.Mvp),
        new(ItemType.MatrixMr, "Matrix Multiple Response", Phase.Mvp),
        new(ItemType.MultipleResponseGrouping, "Multiple Response Grouping", Phase.Mvp),
        new(ItemType.DropdownCloze, "Drop-Down Cloze", Phase.Mvp),
        new(ItemType.DropdownTable, "Drop-Down Table", Phase.Mvp),
        new(ItemType.HighlightText, "Highlight Text", Phase.Mvp),
        new(ItemType.HighlightTable, "Highlight Table", Phase.Mvp),
        new(ItemType.FillBlankCalc, "Fill in the Blank (Calculation)", Phase.Mvp),
        new(ItemType.OrderedResponse, "Ordered Response", Phase.Mvp),
        new(ItemType.DropdownRationale, "Drop-Down Rationale", Phase.P2),
        new(ItemType.DragDropCloze, "Drag-and-Drop Cloze", Phase.P2),
        new(ItemType.DragDropRationale, "Drag-and-Drop Rationale", Phase.P2),
        new(ItemType.Bowtie, "Bowtie", Phase.P2),
        new(ItemType.Hotspot, "Hotspot", Phase.P2),
    ];

    public static ItemTypeDescription Describe(ItemType type) => All.Single(d => d.Type == type);
}
