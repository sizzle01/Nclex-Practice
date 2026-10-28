using System.Text.Json;
using Nclex.Domain.ItemTypes;

namespace Nclex.Domain.Content;

/// <summary>
/// Checks a question's exhibits (interfaces.md section 1, Exhibit): the charts, flowsheets, notes
/// and media shown beside the stem. Trend, chart, audio and video items are any item type plus
/// exhibits (architecture.md 5.4), so these rules apply to every type.
/// </summary>
public static class ExhibitRules
{
    /// <summary>The exhibit kinds the player can show.</summary>
    public static readonly IReadOnlySet<string> Kinds = new HashSet<string>(StringComparer.Ordinal)
    {
        "table", "flowsheet", "image", "audio", "video", "text",
    };

    private static readonly HashSet<string> TableKinds = new(StringComparer.Ordinal) { "table", "flowsheet" };

    /// <summary>The kinds that show a media asset, which must be of the same kind.</summary>
    public static readonly IReadOnlySet<string> MediaKinds = new HashSet<string>(StringComparer.Ordinal) { "image", "audio", "video" };

    /// <summary>
    /// The problems in an exhibits array, and the media each exhibit refers to (by kind), so the
    /// caller can check those assets exist. Paths start at "exhibits".
    /// </summary>
    public static ExhibitCheck Check(JsonElement exhibits)
    {
        var issues = new List<ValidationIssue>();
        var media = new List<MediaReference>();
        if (exhibits.ValueKind != JsonValueKind.Array)
        {
            issues.Add(new ValidationIssue("exhibits", ValidationIssueCodes.InvalidShape, "Exhibits must be a JSON array."));
            return new ExhibitCheck(issues, media);
        }

        var ids = new HashSet<string>(StringComparer.Ordinal);
        var index = 0;
        foreach (var exhibit in exhibits.EnumerateArray())
        {
            CheckExhibit(exhibit, $"exhibits[{index}]", ids, issues, media);
            index++;
        }

        return new ExhibitCheck(issues, media);
    }

    /// <summary>
    /// Media used by graphic options: every "mediaId" anywhere in an item's content (interfaces.md
    /// section 1, Option.mediaId). Their kind is not fixed by the content, so it is left null.
    /// </summary>
    public static IReadOnlyList<MediaReference> MediaInContent(JsonElement content)
    {
        var found = new List<MediaReference>();
        Collect(content, "content", found);
        return found;
    }

    private static void CheckExhibit(JsonElement exhibit, string path, HashSet<string> ids, List<ValidationIssue> issues, List<MediaReference> media)
    {
        if (exhibit.ValueKind != JsonValueKind.Object)
        {
            issues.Add(new ValidationIssue(path, ValidationIssueCodes.InvalidShape, "An exhibit must be a JSON object."));
            return;
        }

        var id = Text(exhibit, "id");
        if (id is null)
        {
            issues.Add(new ValidationIssue($"{path}.id", ValidationIssueCodes.Required, "Every exhibit needs an id."));
        }
        else if (!ids.Add(id))
        {
            issues.Add(new ValidationIssue($"{path}.id", ValidationIssueCodes.DuplicateId, $"Exhibit id \"{id}\" is used more than once."));
        }

        if (Text(exhibit, "title") is null)
        {
            issues.Add(new ValidationIssue($"{path}.title", ValidationIssueCodes.Required, "Every exhibit needs a title; it names the tab and the table."));
        }

        var kind = Text(exhibit, "kind");
        if (kind is null || !Kinds.Contains(kind))
        {
            issues.Add(new ValidationIssue($"{path}.kind", ValidationIssueCodes.InvalidValue, $"The kind must be one of: {string.Join(", ", Kinds)}."));
            return;
        }

        if (TableKinds.Contains(kind))
        {
            CheckTable(exhibit, $"{path}.table", issues);
        }
        else if (MediaKinds.Contains(kind))
        {
            CheckMediaId(exhibit, $"{path}.mediaId", kind, issues, media);
        }
        else if (Text(exhibit, "body") is null)
        {
            issues.Add(new ValidationIssue($"{path}.body", ValidationIssueCodes.Required, "A text exhibit needs a body."));
        }
    }

    // Every row needs a label and exactly one cell per column, or the flowsheet's columns would not line up.
    private static void CheckTable(JsonElement exhibit, string path, List<ValidationIssue> issues)
    {
        if (!exhibit.TryGetProperty("table", out var table) || table.ValueKind != JsonValueKind.Object
            || !table.TryGetProperty("columns", out var columns) || columns.ValueKind != JsonValueKind.Array
            || !table.TryGetProperty("rows", out var rows) || rows.ValueKind != JsonValueKind.Array)
        {
            issues.Add(new ValidationIssue(path, ValidationIssueCodes.Required, "A table or flowsheet needs a table with columns and rows."));
            return;
        }

        var columnCount = columns.GetArrayLength();
        if (columnCount == 0 || rows.GetArrayLength() == 0)
        {
            issues.Add(new ValidationIssue(path, ValidationIssueCodes.TooFew, "A table needs at least one column and one row."));
        }

        var r = 0;
        foreach (var row in rows.EnumerateArray())
        {
            var rowPath = $"{path}.rows[{r}]";
            if (row.ValueKind != JsonValueKind.Object || Text(row, "label") is null)
            {
                issues.Add(new ValidationIssue($"{rowPath}.label", ValidationIssueCodes.Required, "Every row needs a label."));
            }

            var cellCount = row.ValueKind == JsonValueKind.Object && row.TryGetProperty("cells", out var cells) && cells.ValueKind == JsonValueKind.Array
                ? cells.GetArrayLength()
                : -1;
            if (cellCount != columnCount)
            {
                issues.Add(new ValidationIssue($"{rowPath}.cells", ValidationIssueCodes.Mismatch, $"The row needs {columnCount} cell(s), one per column."));
            }

            r++;
        }
    }

    private static void CheckMediaId(JsonElement exhibit, string path, string kind, List<ValidationIssue> issues, List<MediaReference> media)
    {
        if (Text(exhibit, "mediaId") is { } text && Guid.TryParse(text, out var mediaId))
        {
            media.Add(new MediaReference(path, mediaId, kind));
        }
        else
        {
            issues.Add(new ValidationIssue(path, ValidationIssueCodes.Required, $"An {kind} exhibit needs the id of a media asset."));
        }
    }

    private static void Collect(JsonElement element, string path, List<MediaReference> found)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    var childPath = $"{path}.{property.Name}";
                    if (property.Name == "mediaId" && property.Value.ValueKind == JsonValueKind.String)
                    {
                        found.Add(new MediaReference(childPath, Guid.TryParse(property.Value.GetString(), out var id) ? id : Guid.Empty, Kind: null));
                    }
                    else
                    {
                        Collect(property.Value, childPath, found);
                    }
                }

                break;
            case JsonValueKind.Array:
                var i = 0;
                foreach (var item in element.EnumerateArray())
                {
                    Collect(item, $"{path}[{i++}]", found);
                }

                break;
        }
    }

    private static string? Text(JsonElement element, string property) =>
        element.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(value.GetString())
            ? value.GetString()
            : null;
}

/// <summary>What <see cref="ExhibitRules.Check"/> found.</summary>
/// <param name="Issues">Problems with the exhibits themselves.</param>
/// <param name="Media">The media assets the exhibits show.</param>
public sealed record ExhibitCheck(IReadOnlyList<ValidationIssue> Issues, IReadOnlyList<MediaReference> Media);

/// <summary>A place in a question that shows a media asset.</summary>
/// <param name="Path">Where the id is, e.g. "exhibits[1].mediaId".</param>
/// <param name="MediaId">The asset's id; <see cref="Guid.Empty"/> when the text is not a valid id.</param>
/// <param name="Kind">The kind the asset must have, or null when any kind will do.</param>
public sealed record MediaReference(string Path, Guid MediaId, string? Kind);
