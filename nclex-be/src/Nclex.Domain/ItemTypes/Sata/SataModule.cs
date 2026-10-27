namespace Nclex.Domain.ItemTypes.Sata;

/// <summary>
/// Select All That Apply, scored +/- with a floor of 0 (interfaces.md 2.2): each correct selection
/// earns a point, each incorrect selection loses one, and the item can't score below zero.
/// </summary>
public sealed class SataModule : ItemTypeModule<SataContent, SataKey, SataResponse>
{
    private const int MinOptions = 5;
    private const int MaxOptions = 10;

    public override ItemType Type => ItemType.Sata;

    public override Phase Phase => Phase.Mvp;

    public override decimal MaxPoints(SataContent content, SataKey key) => key.Correct.Count;

    public override GradeResult Grade(SataContent content, SataKey key, SataResponse? response)
    {
        var selected = response?.Selected.ToHashSet(StringComparer.Ordinal) ?? [];
        var correct = key.Correct.ToHashSet(StringComparer.Ordinal);
        var detail = content.Options.Select(o => (chosen: selected.Contains(o.Id), ok: correct.Contains(o.Id)) switch
        {
            (true, true) => new GradeDetail(o.Id, +1, Selected: o.Id),
            (true, false) => new GradeDetail(o.Id, -1, Selected: o.Id),
            (false, true) => new GradeDetail(o.Id, 0, Missed: true),
            _ => new GradeDetail(o.Id, 0),
        }).ToList();
        var points = Math.Max(0, detail.Sum(d => d.Delta));
        return GradeResult.From(points, MaxPoints(content, key), detail);
    }

    public override IReadOnlyList<ValidationIssue> ValidateAuthoring(SataContent content, SataKey key, Rationale rationale)
    {
        var issues = OptionRules.CheckOptions(content.Options, MinOptions, MaxOptions).ToList();
        if (key.Correct.Count == 0)
        {
            issues.Add(new ValidationIssue("key.correct", ValidationIssueCodes.TooFew, "At least one option must be correct."));
        }

        issues.AddRange(OptionRules.CheckSelection(key.Correct, content.Options, "key.correct"));
        issues.AddRange(OptionRules.CheckRationale(rationale, content.Options));
        return issues;
    }

    public override IReadOnlyList<ValidationIssue> ValidateResponse(SataContent content, SataResponse response) =>
        [.. OptionRules.CheckSelection(response.Selected, content.Options, "response.selected")];

    public override SataContent Present(SataContent content, string seed) =>
        content with { Options = SeededShuffle.Shuffle(content.Options, seed) };
}
