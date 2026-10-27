namespace Nclex.Domain.ItemTypes;

/// <summary>
/// The NGN item formats (database enum <c>item_type</c>, interfaces.md section 2). Each one is
/// implemented by an <see cref="IItemTypeModule"/>. Trend, exhibit, audio, video and graphic-option
/// items are not separate types: they are a base type plus exhibits or option media (architecture.md 5.4).
/// </summary>
public enum ItemType
{
    MultipleChoice,

    /// <summary>Select All That Apply.</summary>
    Sata,
    SelectN,
    MatrixMc,
    MatrixMr,
    MultipleResponseGrouping,
    DropdownCloze,
    DropdownTable,
    DropdownRationale,
    HighlightText,
    HighlightTable,
    FillBlankCalc,
    OrderedResponse,
    DragDropCloze,
    DragDropRationale,
    Bowtie,
    Hotspot,
}
