using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Application.Common;
using Nclex.Domain.Content;
using Nclex.Domain.Entities;
using Nclex.Domain.ItemTypes;

namespace Nclex.Application.Admin.Questions;

/// <summary>
/// Checks a draft on every save (database.md 2.4, "rules enforced in the application"): the tags
/// exist for the track, the exhibits are well formed and name real media, and the item-type module
/// accepts the content, key and rationale, including a "why" for every option (PRD 5.1). The same
/// checks the content importer runs, answered as 400 VALIDATION_FAILED with a message per problem.
/// </summary>
public sealed class AuthoringValidator(IAppDbContext db, ItemTypeRegistry itemTypes)
{
    /// <summary>The draft as an item ready to save, with max points worked out from the key.</summary>
    /// <param name="draft">What the author sent.</param>
    /// <param name="track">The question's track, which decides the Client Needs tree.</param>
    /// <param name="placement">The version's place in a case study, kept as it was; null for standalone items.</param>
    /// <param name="cancellationToken">Cancels the database reads.</param>
    public async Task<AuthoredItem> ValidateAsync(DraftInput draft, ExamTrack track, CasePlacement? placement, CancellationToken cancellationToken)
    {
        var issues = new List<ValidationIssue>();
        CheckText(draft, issues);
        await CheckTagsAsync(draft, track, issues, cancellationToken);

        var exhibits = draft.Exhibits is { ValueKind: not (JsonValueKind.Undefined or JsonValueKind.Null) } given
            ? given
            : JsonSerializer.SerializeToElement(Array.Empty<object>());
        var exhibitCheck = ExhibitRules.Check(exhibits);
        issues.AddRange(exhibitCheck.Issues);
        var mediaReferences = exhibitCheck.Media.Concat(ExhibitRules.MediaInContent(draft.Content)).ToList();
        await CheckMediaAsync(mediaReferences, issues, cancellationToken);

        var maxPoints = CheckItem(draft, issues);
        if (issues.Count > 0)
        {
            throw Invalid(issues);
        }

        return new AuthoredItem(
            draft.ItemType,
            draft.Stem.Trim(),
            draft.Content,
            draft.AnswerKey,
            draft.Rationale,
            draft.EducationalObjective.Trim(),
            exhibits,
            draft.ClientNeedsId,
            draft.BodySystemId,
            draft.CjmmSkill,
            draft.Difficulty,
            draft.TimeAllotmentSec ?? DraftInput.DefaultTimeAllotmentSec,
            maxPoints,
            placement);
    }

    /// <summary>A 400 VALIDATION_FAILED whose errors[] name each problem's path and explain it.</summary>
    public static AppException Invalid(IEnumerable<ValidationIssue> issues) =>
        new(CommonErrors.ValidationFailed, fieldErrors: [.. issues.Select(i => new AppFieldError(i.Path, i.Code, i.Message))]);

    private static void CheckText(DraftInput draft, List<ValidationIssue> issues)
    {
        if (string.IsNullOrWhiteSpace(draft.Stem))
        {
            issues.Add(new ValidationIssue("stem", ValidationIssueCodes.Required, "Write the question stem."));
        }

        if (string.IsNullOrWhiteSpace(draft.EducationalObjective))
        {
            issues.Add(new ValidationIssue("educationalObjective", ValidationIssueCodes.Required, "Write the educational objective."));
        }

        if (draft.TimeAllotmentSec is <= 0)
        {
            issues.Add(new ValidationIssue("timeAllotmentSec", ValidationIssueCodes.InvalidValue, "The time allotment must be more than 0 seconds."));
        }

        if (!Enum.IsDefined(draft.Difficulty))
        {
            issues.Add(new ValidationIssue("difficulty", ValidationIssueCodes.InvalidValue, "Choose easy, moderate or difficult."));
        }
    }

    // Questions are tagged to a leaf of their own track's Client Needs tree (database.md 2.3).
    private async Task CheckTagsAsync(DraftInput draft, ExamTrack track, List<ValidationIssue> issues, CancellationToken cancellationToken)
    {
        var category = await db.ClientNeedsCategories.AsNoTracking()
            .Where(c => c.Id == draft.ClientNeedsId)
            .Select(c => new { c.Track, IsParent = db.ClientNeedsCategories.Any(child => child.ParentId == c.Id) })
            .SingleOrDefaultAsync(cancellationToken);
        if (category is null || category.Track != track || category.IsParent)
        {
            issues.Add(new ValidationIssue("clientNeedsId", ValidationIssueCodes.InvalidValue, $"Choose a Client Needs subcategory of the {track} track."));
        }

        if (!await db.BodySystems.AnyAsync(b => b.Id == draft.BodySystemId, cancellationToken))
        {
            issues.Add(new ValidationIssue("bodySystemId", ValidationIssueCodes.InvalidValue, "Choose a body system."));
        }
    }

    // Every mediaId must name an asset, of the kind its exhibit shows (an image exhibit, an image).
    private async Task CheckMediaAsync(List<MediaReference> references, List<ValidationIssue> issues, CancellationToken cancellationToken)
    {
        var ids = references.Select(r => r.MediaId).Distinct().ToList();
        var kinds = await db.MediaAssets.AsNoTracking()
            .Where(m => ids.Contains(m.Id))
            .ToDictionaryAsync(m => m.Id, m => m.Kind, cancellationToken);
        foreach (var reference in references)
        {
            if (!kinds.TryGetValue(reference.MediaId, out var kind))
            {
                issues.Add(new ValidationIssue(reference.Path, ValidationIssueCodes.UnknownId, $"Media {reference.MediaId} does not exist. Upload it first."));
            }
            else if (reference.Kind is not null && reference.Kind != kind)
            {
                issues.Add(new ValidationIssue(reference.Path, ValidationIssueCodes.Mismatch, $"An {reference.Kind} exhibit can't show an {kind} file."));
            }
        }
    }

    // Returns the max points, or 0 when the item-type checks found problems.
    private short CheckItem(DraftInput draft, List<ValidationIssue> issues)
    {
        if (!itemTypes.TryGet(draft.ItemType, out var module))
        {
            issues.Add(new ValidationIssue("itemType", ValidationIssueCodes.InvalidValue, $"Item type {draft.ItemType} can't be authored yet."));
            return 0;
        }

        var rationale = ReadRationale(draft.Rationale, issues);
        if (rationale is null)
        {
            return 0;
        }

        // The module names the key "key"; this API calls it answerKey.
        var itemIssues = module.ValidateAuthoring(draft.Content, draft.AnswerKey, rationale)
            .Select(i => i.Path == "key" || i.Path.StartsWith("key.", StringComparison.Ordinal) ? i with { Path = "answerKey" + i.Path[3..] } : i)
            .ToList();
        issues.AddRange(itemIssues);
        return itemIssues.Count > 0 ? (short)0 : (short)module.MaxPoints(draft.Content, draft.AnswerKey);
    }

    private static Rationale? ReadRationale(JsonElement json, List<ValidationIssue> issues)
    {
        if (json.ValueKind != JsonValueKind.Object)
        {
            issues.Add(new ValidationIssue("rationale", ValidationIssueCodes.InvalidShape, "The rationale must be an object with correct and perOption."));
            return null;
        }

        try
        {
            var rationale = json.Deserialize<Rationale>(ItemJson.Options);
            if (string.IsNullOrWhiteSpace(rationale?.Correct))
            {
                issues.Add(new ValidationIssue("rationale.correct", ValidationIssueCodes.Required, "Explain why the correct answer is correct."));
            }

            return rationale;
        }
        catch (JsonException ex)
        {
            issues.Add(new ValidationIssue("rationale", ValidationIssueCodes.InvalidShape, $"The rationale does not have the expected fields: {ex.Message}"));
            return null;
        }
    }
}
