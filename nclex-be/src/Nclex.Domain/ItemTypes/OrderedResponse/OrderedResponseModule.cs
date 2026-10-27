namespace Nclex.Domain.ItemTypes.OrderedResponse;

/// <summary>
/// Ordered response, e.g. the steps of a procedure. Scored 1 point for the full sequence by default,
/// or 1 per correctly placed item (interfaces.md 2.9).
/// </summary>
public sealed class OrderedResponseModule : ItemTypeModule<OrderedResponseContent, OrderedResponseKey, OrderedResponseResponse>
{
    private const int MinItems = 3;

    public override ItemType Type => ItemType.OrderedResponse;

    public override Phase Phase => Phase.Mvp;

    public override decimal MaxPoints(OrderedResponseContent content, OrderedResponseKey key) =>
        key.Scoring == OrderedScoring.PerPosition ? key.Order.Count : 1;

    /// <remarks>
    /// The detail has one entry per position, targeted at the item that belongs there, with delta 1
    /// when the candidate put it there. With all-or-nothing scoring the deltas still show which
    /// positions were right, but only a fully correct order earns the point.
    /// </remarks>
    public override GradeResult Grade(OrderedResponseContent content, OrderedResponseKey key, OrderedResponseResponse? response)
    {
        var given = response?.Order ?? [];
        var detail = key.Order.Select((expected, position) =>
        {
            var placed = position < given.Count ? given[position] : null;
            return placed == expected
                ? new GradeDetail(expected, +1, Selected: placed, Expected: expected)
                : new GradeDetail(expected, 0, Selected: placed, Expected: expected, Missed: true);
        }).ToList();

        var inPlace = detail.Sum(d => d.Delta);
        var points = key.Scoring == OrderedScoring.PerPosition
            ? inPlace
            : (inPlace == key.Order.Count && given.Count == key.Order.Count ? 1 : 0);
        return GradeResult.From(points, MaxPoints(content, key), detail);
    }

    public override IReadOnlyList<ValidationIssue> ValidateAuthoring(OrderedResponseContent content, OrderedResponseKey key, Rationale rationale)
    {
        var issues = OptionRules.CheckOptions(content.Items, MinItems, path: "content.items").ToList();
        issues.AddRange(CheckPermutation(content, key.Order, "key.order"));
        issues.AddRange(OptionRules.CheckRationale(rationale, content.Items));
        return issues;
    }

    /// <summary>The player always holds every item, so a saved order must be a full permutation.</summary>
    public override IReadOnlyList<ValidationIssue> ValidateResponse(OrderedResponseContent content, OrderedResponseResponse response) =>
        [.. CheckPermutation(content, response.Order, path: "response.order")];

    public override OrderedResponseContent Present(OrderedResponseContent content, string seed) =>
        content with { Items = SeededShuffle.Shuffle(content.Items, seed) };

    private static IEnumerable<ValidationIssue> CheckPermutation(OrderedResponseContent content, IReadOnlyList<string> order, string path)
    {
        var itemIds = content.Items.Select(i => i.Id).ToHashSet(StringComparer.Ordinal);
        foreach (var issue in IdRules.CheckReferences(order, itemIds, path, "item"))
        {
            yield return issue;
        }

        var missing = itemIds.Except(order, StringComparer.Ordinal).ToList();
        if (missing.Count > 0)
        {
            yield return new ValidationIssue(path, ValidationIssueCodes.TooFew, $"Every item must be placed; missing: {string.Join(", ", missing)}.");
        }
    }
}
