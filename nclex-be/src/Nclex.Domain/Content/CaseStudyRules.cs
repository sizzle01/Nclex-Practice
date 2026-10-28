using System.Text.Json;
using Nclex.Domain.Entities;
using Nclex.Domain.ItemTypes;

namespace Nclex.Domain.Content;

/// <summary>
/// Rules for NGN unfolding case studies (database.md 2.4, case_studies): the shape of a scenario
/// (interfaces.md section 1, CaseStudyScenario), the positions of a case's items, and the order
/// those items take in a curated test set.
/// </summary>
public static class CaseStudyRules
{
    /// <summary>
    /// The problems in a scenario, and the media its exhibits show. Each tab needs an id, a title and
    /// at least one exhibit, and its exhibits follow <see cref="ExhibitRules"/>. Paths start at "scenario".
    /// </summary>
    public static ExhibitCheck CheckScenario(JsonElement scenario)
    {
        var issues = new List<ValidationIssue>();
        var media = new List<MediaReference>();
        if (scenario.ValueKind != JsonValueKind.Object
            || !scenario.TryGetProperty("tabs", out var tabs)
            || tabs.ValueKind != JsonValueKind.Array)
        {
            issues.Add(new ValidationIssue("scenario", ValidationIssueCodes.InvalidShape, "A scenario is an object with a tabs array."));
            return new ExhibitCheck(issues, media);
        }

        if (tabs.GetArrayLength() == 0)
        {
            issues.Add(new ValidationIssue("scenario.tabs", ValidationIssueCodes.TooFew, "A scenario needs at least one tab."));
        }

        var ids = new HashSet<string>(StringComparer.Ordinal);
        var index = 0;
        foreach (var tab in tabs.EnumerateArray())
        {
            CheckTab(tab, $"scenario.tabs[{index}]", ids, issues, media);
            index++;
        }

        return new ExhibitCheck(issues, media);
    }

    /// <summary>
    /// The ids of a scenario's tabs. The player shows them beside the item's own exhibits, so the
    /// two must not share an id (exhibitTabs in the frontend).
    /// </summary>
    public static IReadOnlySet<string> TabIds(JsonElement scenario)
    {
        var ids = new HashSet<string>(StringComparer.Ordinal);
        if (scenario.ValueKind == JsonValueKind.Object
            && scenario.TryGetProperty("tabs", out var tabs)
            && tabs.ValueKind == JsonValueKind.Array)
        {
            foreach (var tab in tabs.EnumerateArray())
            {
                if (Text(tab, "id") is { } id)
                {
                    ids.Add(id);
                }
            }
        }

        return ids;
    }

    /// <summary>
    /// True when a case's positions run 1, 2, … n with no gap or repeat, so the player can say
    /// "question 2 of 3" and the custom builder can keep the items in order (SetSampler).
    /// </summary>
    public static bool PositionsAreComplete(IEnumerable<short> positions)
    {
        var sorted = positions.Order().ToList();
        return sorted.Count <= CaseStudy.MaxItems && sorted.Select((p, i) => p == i + 1).All(ok => ok);
    }

    /// <summary>
    /// The places where a curated set breaks a case study apart. A set that uses any item of a case
    /// must have all of them, back to back, in case order, as the custom builder does (SetSampler);
    /// otherwise the player would show a case's tabs with items missing or out of order.
    /// </summary>
    /// <param name="questionIds">The set's questions, in the order they are served.</param>
    /// <param name="placementOf">Where a question sits in a case, or null for a standalone question.</param>
    /// <param name="caseSize">How many items a case has.</param>
    /// <returns>For each broken case, the index of its first item in the set and a message.</returns>
    public static IReadOnlyList<(int Index, string Message)> CheckSetOrder(
        IReadOnlyList<Guid> questionIds,
        Func<Guid, CasePlacement?> placementOf,
        Func<Guid, int> caseSize)
    {
        var problems = new List<(int, string)>();
        var cases = questionIds
            .Select((id, index) => (Index: index, Placement: placementOf(id)))
            .Where(x => x.Placement is not null)
            .GroupBy(x => x.Placement!.CaseStudyId);
        foreach (var items in cases)
        {
            var list = items.ToList();
            var size = caseSize(items.Key);
            var backToBackInOrder = list.Select((x, k) => x.Index == list[0].Index + k && x.Placement!.Position == k + 1).All(ok => ok);
            if (list.Count != size || !backToBackInOrder)
            {
                problems.Add((list[0].Index, $"Case study {items.Key} must appear whole: all {size} of its questions, back to back, in case order."));
            }
        }

        return problems;
    }

    private static void CheckTab(JsonElement tab, string path, HashSet<string> ids, List<ValidationIssue> issues, List<MediaReference> media)
    {
        if (tab.ValueKind != JsonValueKind.Object)
        {
            issues.Add(new ValidationIssue(path, ValidationIssueCodes.InvalidShape, "A tab must be a JSON object."));
            return;
        }

        var id = Text(tab, "id");
        if (id is null)
        {
            issues.Add(new ValidationIssue($"{path}.id", ValidationIssueCodes.Required, "Every tab needs an id."));
        }
        else if (!ids.Add(id))
        {
            issues.Add(new ValidationIssue($"{path}.id", ValidationIssueCodes.DuplicateId, $"Tab id \"{id}\" is used more than once."));
        }

        if (Text(tab, "title") is null)
        {
            issues.Add(new ValidationIssue($"{path}.title", ValidationIssueCodes.Required, "Every tab needs a title, e.g. \"Nurses' notes\"."));
        }

        if (!tab.TryGetProperty("exhibits", out var exhibits) || exhibits.ValueKind != JsonValueKind.Array || exhibits.GetArrayLength() == 0)
        {
            issues.Add(new ValidationIssue($"{path}.exhibits", ValidationIssueCodes.TooFew, "A tab needs at least one exhibit."));
            return;
        }

        var check = ExhibitRules.Check(exhibits);
        issues.AddRange(check.Issues.Select(issue => issue with { Path = $"{path}.{issue.Path}" }));
        media.AddRange(check.Media.Select(reference => reference with { Path = $"{path}.{reference.Path}" }));
    }

    private static string? Text(JsonElement element, string property) =>
        element.ValueKind == JsonValueKind.Object
        && element.TryGetProperty(property, out var value)
        && value.ValueKind == JsonValueKind.String
        && !string.IsNullOrWhiteSpace(value.GetString())
            ? value.GetString()
            : null;
}
