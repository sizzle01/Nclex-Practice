namespace Nclex.Domain.ItemTypes.SelectN;

/// <summary>
/// Select N, e.g. "Select the 3 findings that...". The PRD lists it as "0/1"; like the NCSBN, this
/// design applies 0/1 per option, so each correct selection earns a point and wrong ones cost nothing
/// (interfaces.md 2.3). A key with <c>scoringOverride: all_or_nothing</c> scores a single point instead.
/// </summary>
public sealed class SelectNModule : ItemTypeModule<SelectNContent, SelectNKey, SelectNResponse>
{
    public override ItemType Type => ItemType.SelectN;

    public override Phase Phase => Phase.Mvp;

    public override decimal MaxPoints(SelectNContent content, SelectNKey key) =>
        key.ScoringOverride == SelectNScoringOverride.AllOrNothing ? 1 : content.SelectCount;

    public override GradeResult Grade(SelectNContent content, SelectNKey key, SelectNResponse? response)
    {
        var selected = response?.Selected.ToHashSet(StringComparer.Ordinal) ?? [];
        var correct = key.Correct.ToHashSet(StringComparer.Ordinal);
        var detail = content.Options.Select(o => (chosen: selected.Contains(o.Id), ok: correct.Contains(o.Id)) switch
        {
            (true, true) => new GradeDetail(o.Id, +1, Selected: o.Id),
            (true, false) => new GradeDetail(o.Id, 0, Selected: o.Id),
            (false, true) => new GradeDetail(o.Id, 0, Missed: true),
            _ => new GradeDetail(o.Id, 0),
        }).ToList();

        var hits = detail.Sum(d => d.Delta);
        var points = key.ScoringOverride == SelectNScoringOverride.AllOrNothing
            ? (hits == content.SelectCount && selected.Count == content.SelectCount ? 1 : 0)
            : hits;
        return GradeResult.From(points, MaxPoints(content, key), detail);
    }

    public override IReadOnlyList<ValidationIssue> ValidateAuthoring(SelectNContent content, SelectNKey key, Rationale rationale)
    {
        var issues = OptionRules.CheckOptions(content.Options, min: 3).ToList();

        // Picking every option, or none, would not test anything.
        if (content.SelectCount < 1 || content.SelectCount >= content.Options.Count)
        {
            issues.Add(new ValidationIssue("content.selectCount", ValidationIssueCodes.InvalidValue, "The select count must be at least 1 and less than the number of options."));
        }

        if (key.Correct.Count != content.SelectCount)
        {
            issues.Add(new ValidationIssue("key.correct", ValidationIssueCodes.Mismatch, $"The key must list exactly {content.SelectCount} options, the select count."));
        }

        issues.AddRange(OptionRules.CheckSelection(key.Correct, content.Options, "key.correct"));
        issues.AddRange(OptionRules.CheckRationale(rationale, content.Options));
        return issues;
    }

    public override IReadOnlyList<ValidationIssue> ValidateResponse(SelectNContent content, SelectNResponse response)
    {
        var issues = OptionRules.CheckSelection(response.Selected, content.Options, "response.selected").ToList();
        if (response.Selected.Count > content.SelectCount)
        {
            issues.Add(new ValidationIssue("response.selected", ValidationIssueCodes.TooMany, $"Select at most {content.SelectCount} options."));
        }

        return issues;
    }

    public override SelectNContent Present(SelectNContent content, string seed) =>
        content with { Options = SeededShuffle.Shuffle(content.Options, seed) };
}
