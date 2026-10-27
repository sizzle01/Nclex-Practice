namespace Nclex.Domain.ItemTypes.Grouped;

/// <summary>
/// Grading and key/response checks shared by matrix multiple response and multiple response grouping
/// (interfaces.md 2.5): each group is scored +/- with its own floor of 0, so a wrong pick in one group
/// can't cancel a correct pick in another.
/// </summary>
public static class GroupedGrader
{
    public static decimal MaxPoints(GroupedKey key) => key.ByGroup.Values.Sum(correct => correct.Count);

    public static GradeResult Grade(IReadOnlyList<ChoiceGroup> groups, GroupedKey key, GroupedResponse? response)
    {
        var points = 0;
        var detail = new List<GradeDetail>();
        foreach (var group in groups)
        {
            var correct = key.ByGroup.GetValueOrDefault(group.Id) ?? [];
            var selected = response?.ByGroup.GetValueOrDefault(group.Id) ?? [];
            var (groupPoints, groupDetail) = PlusMinusScoring.Score(group.ChoiceIds, correct, selected);
            points += groupPoints;
            detail.AddRange(groupDetail);
        }

        return GradeResult.From(points, MaxPoints(key), detail);
    }

    /// <summary>
    /// Every group needs at least one correct choice, so each group is worth something, and the key
    /// may only name real groups and choices that belong to them.
    /// </summary>
    public static IEnumerable<ValidationIssue> CheckKey(IReadOnlyList<ChoiceGroup> groups, GroupedKey key, string choiceNoun)
    {
        foreach (var group in groups)
        {
            if (key.ByGroup.GetValueOrDefault(group.Id) is not { Count: > 0 })
            {
                yield return new ValidationIssue($"key.byGroup.{group.Id}", ValidationIssueCodes.TooFew, $"Group \"{group.Id}\" needs at least one correct {choiceNoun}.");
            }
        }

        foreach (var issue in CheckSelections(groups, key.ByGroup, "key.byGroup", choiceNoun))
        {
            yield return issue;
        }
    }

    /// <summary>Each group in a response must exist, and its selections must belong to it.</summary>
    public static IEnumerable<ValidationIssue> CheckResponse(IReadOnlyList<ChoiceGroup> groups, GroupedResponse response, string choiceNoun) =>
        CheckSelections(groups, response.ByGroup, "response.byGroup", choiceNoun);

    private static IEnumerable<ValidationIssue> CheckSelections(
        IReadOnlyList<ChoiceGroup> groups,
        IReadOnlyDictionary<string, IReadOnlyList<string>> byGroup,
        string path,
        string choiceNoun)
    {
        var groupsById = groups.ToDictionary(g => g.Id, StringComparer.Ordinal);
        foreach (var (groupId, ids) in byGroup)
        {
            if (!groupsById.TryGetValue(groupId, out var group))
            {
                yield return new ValidationIssue($"{path}.{groupId}", ValidationIssueCodes.UnknownId, $"There is no group \"{groupId}\".");
                continue;
            }

            var inGroup = group.ChoiceIds.ToHashSet(StringComparer.Ordinal);
            foreach (var issue in IdRules.CheckReferences(ids, inGroup, $"{path}.{groupId}", $"{choiceNoun} in group \"{groupId}\""))
            {
                yield return issue;
            }
        }
    }
}
