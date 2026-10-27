using System.Text.RegularExpressions;

namespace Nclex.Domain.ItemTypes.Dropdown;

/// <summary>
/// Drop-down cloze: a sentence or paragraph with drop-down blanks, scored 0/1 per blank
/// (interfaces.md 2.6). Shares <see cref="BlankGrader"/> with drop-down table.
/// </summary>
public sealed partial class DropdownClozeModule : ItemTypeModule<DropdownClozeContent, DropdownKey, DropdownResponse>
{
    public override ItemType Type => ItemType.DropdownCloze;

    public override Phase Phase => Phase.Mvp;

    public override decimal MaxPoints(DropdownClozeContent content, DropdownKey key) => BlankGrader.MaxPoints(content.Blanks);

    public override GradeResult Grade(DropdownClozeContent content, DropdownKey key, DropdownResponse? response) =>
        BlankGrader.Grade(content.Blanks, key, response);

    public override IReadOnlyList<ValidationIssue> ValidateAuthoring(DropdownClozeContent content, DropdownKey key, Rationale rationale)
    {
        var issues = new List<ValidationIssue>();
        if (string.IsNullOrWhiteSpace(content.Template))
        {
            issues.Add(new ValidationIssue("content.template", ValidationIssueCodes.Required, "The template is empty."));
        }

        issues.AddRange(BlankGrader.CheckBlanks(content.Blanks));
        var placements = Placeholder().Matches(content.Template).Select(m => (m.Groups["id"].Value, "content.template")).ToList();
        issues.AddRange(BlankGrader.CheckPlacements(content.Blanks, placements));
        issues.AddRange(BlankGrader.CheckKey(content.Blanks, key));
        issues.AddRange(IdRules.CheckRationale(rationale, BlankGrader.RationaleIds(content.Blanks)));
        return issues;
    }

    public override IReadOnlyList<ValidationIssue> ValidateResponse(DropdownClozeContent content, DropdownResponse response) =>
        [.. BlankGrader.CheckResponse(content.Blanks, response)];

    // "{{b1}}" in the template marks where blank "b1" goes (interfaces.md 2.6).
    [GeneratedRegex(@"\{\{(?<id>[^{}]+)\}\}")]
    private static partial Regex Placeholder();
}
