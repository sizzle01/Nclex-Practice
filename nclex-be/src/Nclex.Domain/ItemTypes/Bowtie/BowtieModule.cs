namespace Nclex.Domain.ItemTypes.Bowtie;

/// <summary>
/// Bowtie (interfaces.md 2.x, bowtie): the candidate fills five slots, one condition, two actions
/// to take and two parameters to monitor. Each slot holding a correct option earns a point, and a
/// wrong one costs nothing, so the maximum is always 5 (the NCSBN's 0/1 scoring for bowties).
/// </summary>
public sealed class BowtieModule : ItemTypeModule<BowtieContent, BowtieKey, BowtieResponse>
{
    /// <summary>The real exam shows 4 conditions and 5 actions and parameters; fewer would make guessing too easy.</summary>
    public const int MinOptionsPerList = 4;

    /// <summary>Enough choice without the lists outgrowing the screen.</summary>
    public const int MaxOptionsPerList = 8;

    /// <summary>The bowtie's two left slots (actions) and two right slots (parameters).</summary>
    public const int SlotsPerWing = 2;

    private const decimal SlotCount = 1 + SlotsPerWing + SlotsPerWing;

    public override ItemType Type => ItemType.Bowtie;

    public override Phase Phase => Phase.P2;

    public override decimal MaxPoints(BowtieContent content, BowtieKey key) => SlotCount;

    public override GradeResult Grade(BowtieContent content, BowtieKey key, BowtieResponse? response)
    {
        IReadOnlyList<string> chosenCondition = response?.Condition is { } condition ? [condition] : [];
        var detail = GradeList(content.Condition, [key.Condition], chosenCondition)
            .Concat(GradeList(content.Actions, key.Actions, response?.Actions ?? []))
            .Concat(GradeList(content.Parameters, key.Parameters, response?.Parameters ?? []))
            .ToList();
        return GradeResult.From(detail.Sum(d => d.Delta), SlotCount, detail);
    }

    public override IReadOnlyList<ValidationIssue> ValidateAuthoring(BowtieContent content, BowtieKey key, Rationale rationale)
    {
        var issues = new List<ValidationIssue>();
        foreach (var (options, name) in Lists(content))
        {
            issues.AddRange(OptionRules.CheckOptions(options, MinOptionsPerList, MaxOptionsPerList, $"content.{name}"));
        }

        // rationale.perOption is keyed by option id alone, so an id may appear in only one list.
        var allIds = AllOptionIds(content);
        issues.AddRange(allIds
            .GroupBy(id => id, StringComparer.Ordinal)
            .Where(g => g.Count() > 1)
            .Select(g => new ValidationIssue("content", ValidationIssueCodes.DuplicateId, $"Option id \"{g.Key}\" is used in more than one list.")));

        issues.AddRange(OptionRules.CheckSelection([key.Condition], content.Condition, "key.condition"));
        issues.AddRange(CheckKeyWing(key.Actions, content.Actions, "key.actions"));
        issues.AddRange(CheckKeyWing(key.Parameters, content.Parameters, "key.parameters"));
        issues.AddRange(IdRules.CheckRationale(rationale, allIds));
        return issues;
    }

    public override IReadOnlyList<ValidationIssue> ValidateResponse(BowtieContent content, BowtieResponse response)
    {
        var issues = new List<ValidationIssue>();
        if (response.Condition is { } condition)
        {
            issues.AddRange(OptionRules.CheckSelection([condition], content.Condition, "response.condition"));
        }

        issues.AddRange(CheckResponseWing(response.Actions, content.Actions, "response.actions"));
        issues.AddRange(CheckResponseWing(response.Parameters, content.Parameters, "response.parameters"));
        return issues;
    }

    // Each list gets its own seed, so the three lists are not shuffled in step with each other.
    public override BowtieContent Present(BowtieContent content, string seed) =>
        new(
            SeededShuffle.Shuffle(content.Condition, $"{seed}:condition"),
            SeededShuffle.Shuffle(content.Actions, $"{seed}:actions"),
            SeededShuffle.Shuffle(content.Parameters, $"{seed}:parameters"));

    // One detail per option, in content order: +1 for a correct option placed, 0 for a wrong one
    // placed, and "missed" for a correct option left out. The review page marks options from these.
    private static IEnumerable<GradeDetail> GradeList(IReadOnlyList<Option> options, IReadOnlyList<string> correct, IReadOnlyList<string> chosen) =>
        options.Select(o => (placed: chosen.Contains(o.Id, StringComparer.Ordinal), ok: correct.Contains(o.Id, StringComparer.Ordinal)) switch
        {
            (true, true) => new GradeDetail(o.Id, +1, Selected: o.Id),
            (true, false) => new GradeDetail(o.Id, 0, Selected: o.Id),
            (false, true) => new GradeDetail(o.Id, 0, Missed: true),
            _ => new GradeDetail(o.Id, 0),
        });

    private static List<ValidationIssue> CheckKeyWing(IReadOnlyList<string> ids, IReadOnlyList<Option> options, string path)
    {
        var issues = OptionRules.CheckSelection(ids, options, path).ToList();
        if (ids.Count != SlotsPerWing)
        {
            issues.Add(new ValidationIssue(path, ValidationIssueCodes.Mismatch, $"The key must list exactly {SlotsPerWing} options here, one per slot."));
        }

        return issues;
    }

    private static List<ValidationIssue> CheckResponseWing(IReadOnlyList<string> ids, IReadOnlyList<Option> options, string path)
    {
        var issues = OptionRules.CheckSelection(ids, options, path).ToList();
        if (ids.Count > SlotsPerWing)
        {
            issues.Add(new ValidationIssue(path, ValidationIssueCodes.TooMany, $"Place at most {SlotsPerWing} options here."));
        }

        return issues;
    }

    private static IEnumerable<(IReadOnlyList<Option> Options, string Name)> Lists(BowtieContent content) =>
        [(content.Condition, "condition"), (content.Actions, "actions"), (content.Parameters, "parameters")];

    private static List<string> AllOptionIds(BowtieContent content) =>
        [.. Lists(content).SelectMany(list => list.Options).Select(o => o.Id)];
}
