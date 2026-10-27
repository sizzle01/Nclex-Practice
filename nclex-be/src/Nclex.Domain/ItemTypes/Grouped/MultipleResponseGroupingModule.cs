namespace Nclex.Domain.ItemTypes.Grouped;

/// <summary>
/// Multiple response grouping: several labelled groups of options, each scored +/- with a floor of 0
/// (interfaces.md 2.5). Shares <see cref="GroupedGrader"/> with matrix multiple response.
/// </summary>
public sealed class MultipleResponseGroupingModule : ItemTypeModule<GroupingContent, GroupedKey, GroupedResponse>
{
    private const int MinOptionsPerGroup = 2;

    public override ItemType Type => ItemType.MultipleResponseGrouping;

    public override Phase Phase => Phase.Mvp;

    public override decimal MaxPoints(GroupingContent content, GroupedKey key) => GroupedGrader.MaxPoints(key);

    public override GradeResult Grade(GroupingContent content, GroupedKey key, GroupedResponse? response) =>
        GroupedGrader.Grade(Groups(content), key, response);

    public override IReadOnlyList<ValidationIssue> ValidateAuthoring(GroupingContent content, GroupedKey key, Rationale rationale)
    {
        var issues = new List<ValidationIssue>();
        if (content.Groups.Count < 1)
        {
            issues.Add(new ValidationIssue("content.groups", ValidationIssueCodes.TooFew, "At least 1 group is needed."));
        }

        issues.AddRange(IdRules.CheckDeclared([.. content.Groups.Select(g => g.Id)], i => $"content.groups[{i}].id", "group"));
        for (var g = 0; g < content.Groups.Count; g++)
        {
            issues.AddRange(CheckGroup(content.Groups[g], $"content.groups[{g}]"));
        }

        // Option ids must be unique across the whole item, because rationale.perOption is keyed by option id alone.
        var options = AllOptions(content);
        issues.AddRange(IdRules.CheckDeclared([.. options.Select(o => o.Option.Id)], i => $"{options[i].Path}.id", "option"));

        issues.AddRange(GroupedGrader.CheckKey(Groups(content), key, "option"));
        issues.AddRange(IdRules.CheckRationale(rationale, [.. options.Select(o => o.Option.Id)]));
        return issues;
    }

    public override IReadOnlyList<ValidationIssue> ValidateResponse(GroupingContent content, GroupedResponse response) =>
        [.. GroupedGrader.CheckResponse(Groups(content), response, "option")];

    /// <summary>Shuffles the options inside each group; the groups keep their authored order.</summary>
    public override GroupingContent Present(GroupingContent content, string seed) =>
        content with
        {
            Groups = [.. content.Groups.Select(g => g with { Options = SeededShuffle.Shuffle(g.Options, $"{seed}:{g.Id}") })],
        };

    private static IEnumerable<ValidationIssue> CheckGroup(OptionGroup group, string path)
    {
        if (string.IsNullOrWhiteSpace(group.Label))
        {
            yield return new ValidationIssue($"{path}.label", ValidationIssueCodes.Required, "Every group needs a label.");
        }

        if (group.Options.Count < MinOptionsPerGroup)
        {
            yield return new ValidationIssue($"{path}.options", ValidationIssueCodes.TooFew, $"Each group needs at least {MinOptionsPerGroup} options.");
        }

        for (var i = 0; i < group.Options.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(group.Options[i].Text))
            {
                yield return new ValidationIssue($"{path}.options[{i}].text", ValidationIssueCodes.Required, "Every option needs text.");
            }
        }
    }

    private static List<(Option Option, string Path)> AllOptions(GroupingContent content) =>
        [.. content.Groups.SelectMany((g, gi) => g.Options.Select((o, oi) => (o, $"content.groups[{gi}].options[{oi}]")))];

    private static List<ChoiceGroup> Groups(GroupingContent content) =>
        [.. content.Groups.Select(g => new ChoiceGroup(g.Id, [.. g.Options.Select(o => o.Id)]))];
}
