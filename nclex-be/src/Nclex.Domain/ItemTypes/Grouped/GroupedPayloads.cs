namespace Nclex.Domain.ItemTypes.Grouped;

/// <summary>
/// The correct choices of each group, as group id to choice ids (interfaces.md 2.5). For matrix items
/// a choice is a cell, "rowId:columnId"; for multiple response grouping it is an option id.
/// </summary>
public sealed record GroupedKey(IReadOnlyDictionary<string, IReadOnlyList<string>> ByGroup);

/// <summary>The choices the candidate selected in each group; a group may be absent when nothing is selected in it.</summary>
public sealed record GroupedResponse(IReadOnlyDictionary<string, IReadOnlyList<string>> ByGroup);

/// <summary>What candidates see in a multiple response grouping item: groups of options (interfaces.md 2.5).</summary>
public sealed record GroupingContent(IReadOnlyList<OptionGroup> Groups);

/// <summary>One group of a multiple response grouping item, e.g. "Medications" with its options.</summary>
public sealed record OptionGroup(string Id, string Label, IReadOnlyList<Option> Options);

/// <summary>A group as the grader sees it: its id and the ids a candidate can select in it.</summary>
public sealed record ChoiceGroup(string Id, IReadOnlyList<string> ChoiceIds);
