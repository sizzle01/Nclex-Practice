namespace Nclex.Domain.ItemTypes.MultipleChoice;

/// <summary>Single-answer multiple choice, scored 0 or 1 (interfaces.md 2.1).</summary>
public sealed class MultipleChoiceModule : ItemTypeModule<MultipleChoiceContent, MultipleChoiceKey, MultipleChoiceResponse>
{
    private const int MinOptions = 2;

    public override ItemType Type => ItemType.MultipleChoice;

    public override Phase Phase => Phase.Mvp;

    public override decimal MaxPoints(MultipleChoiceContent content, MultipleChoiceKey key) => 1;

    public override GradeResult Grade(MultipleChoiceContent content, MultipleChoiceKey key, MultipleChoiceResponse? response)
    {
        var selected = response?.Selected;
        var detail = content.Options.Select(o => (chosen: o.Id == selected, correct: o.Id == key.Correct) switch
        {
            (true, true) => new GradeDetail(o.Id, +1, Selected: o.Id),
            (true, false) => new GradeDetail(o.Id, 0, Selected: o.Id),
            (false, true) => new GradeDetail(o.Id, 0, Missed: true),
            _ => new GradeDetail(o.Id, 0),
        }).ToList();
        var points = selected == key.Correct ? 1 : 0;
        return GradeResult.From(points, MaxPoints(content, key), detail);
    }

    public override IReadOnlyList<ValidationIssue> ValidateAuthoring(MultipleChoiceContent content, MultipleChoiceKey key, Rationale rationale)
    {
        var issues = OptionRules.CheckOptions(content.Options, MinOptions).ToList();
        if (!content.Options.Any(o => o.Id == key.Correct))
        {
            issues.Add(new ValidationIssue("key.correct", ValidationIssueCodes.UnknownId, $"There is no option \"{key.Correct}\"."));
        }

        issues.AddRange(OptionRules.CheckRationale(rationale, content.Options));
        return issues;
    }

    public override IReadOnlyList<ValidationIssue> ValidateResponse(MultipleChoiceContent content, MultipleChoiceResponse response) =>
        response.Selected is null ? [] : [.. OptionRules.CheckSelection([response.Selected], content.Options, "response.selected")];

    public override MultipleChoiceContent Present(MultipleChoiceContent content, string seed) =>
        content with { Options = SeededShuffle.Shuffle(content.Options, seed) };
}
