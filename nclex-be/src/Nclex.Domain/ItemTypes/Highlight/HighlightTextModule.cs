namespace Nclex.Domain.ItemTypes.Highlight;

/// <summary>
/// Highlight text: candidates highlight the parts of a passage that answer the question, scored +/-
/// with a floor of 0 (interfaces.md 2.7).
/// </summary>
public sealed class HighlightTextModule : ItemTypeModule<HighlightTextContent, HighlightKey, HighlightResponse>
{
    private const string Noun = "segment";

    public override ItemType Type => ItemType.HighlightText;

    public override Phase Phase => Phase.Mvp;

    public override decimal MaxPoints(HighlightTextContent content, HighlightKey key) => key.Correct.Count;

    public override GradeResult Grade(HighlightTextContent content, HighlightKey key, HighlightResponse? response) =>
        HighlightRules.Grade(HighlightRules.SelectableIds(Parts(content)), key, response);

    public override IReadOnlyList<ValidationIssue> ValidateAuthoring(HighlightTextContent content, HighlightKey key, Rationale rationale)
    {
        var issues = IdRules.CheckDeclared([.. content.Segments.Select(s => s.Id)], i => $"content.segments[{i}].id", Noun).ToList();
        for (var i = 0; i < content.Segments.Count; i++)
        {
            if (string.IsNullOrEmpty(content.Segments[i].Text))
            {
                issues.Add(new ValidationIssue($"content.segments[{i}].text", ValidationIssueCodes.Required, "Every segment needs text."));
            }
        }

        issues.AddRange(HighlightRules.CheckKeyAndRationale(Parts(content), key, rationale, Noun));
        return issues;
    }

    public override IReadOnlyList<ValidationIssue> ValidateResponse(HighlightTextContent content, HighlightResponse response) =>
        [.. HighlightRules.CheckSelection(Parts(content), response.Selected, "response.selected", Noun)];

    private static List<(string Id, bool Selectable)> Parts(HighlightTextContent content) =>
        [.. content.Segments.Select(s => (s.Id, s.Selectable))];
}
