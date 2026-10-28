using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Domain.Content;
using Nclex.Domain.Entities;
using Nclex.Domain.ItemTypes;

namespace Nclex.Application.Content.ImportQuestions;

/// <summary>
/// Loads questions from an import file and publishes them, along with the file's banks and test
/// sets (see <see cref="CatalogImporter"/>). The whole file is validated first and
/// written in one transaction, so a file with any problem changes nothing. Importing the same file
/// again changes nothing either: a question gets a new version only when its content changed.
/// </summary>
/// <remarks>
/// The file stands in for the review workflow that admin authoring adds in F14, so it is meant for
/// content the team has already reviewed, or for clearly marked sample content.
/// </remarks>
public sealed class ImportQuestionsHandler(
    IAppDbContext db,
    ItemTypeRegistry itemTypes,
    CatalogImporter catalog,
    MediaImporter mediaImporter,
    TimeProvider clock)
{
    /// <summary>Imports a file that has no files beside it, so it can't list media.</summary>
    public Task<ImportResult> HandleAsync(QuestionImportFile file, CancellationToken cancellationToken) =>
        HandleAsync(file, NoContentFiles.Instance, cancellationToken);

    /// <summary>Imports a file whose media files are read from <paramref name="files"/>.</summary>
    public async Task<ImportResult> HandleAsync(QuestionImportFile file, IContentFiles files, CancellationToken cancellationToken)
    {
        var tags = await LoadTagsAsync(file.Track, cancellationToken);
        var ids = file.Questions.Select(q => q.Id).ToList();
        var existing = await db.Questions
            .Where(q => ids.Contains(q.Id))
            .ToDictionaryAsync(q => q.Id, cancellationToken);

        var validated = Validate(file, tags, existing, out var issues, out var mediaReferences);
        var media = await mediaImporter.ValidateAsync(file.Media, mediaReferences, files, issues, cancellationToken);
        await catalog.ValidateAsync(file, issues, cancellationToken);
        if (issues.Count > 0)
        {
            return ImportResult.Failed(issues);
        }

        var now = clock.GetUtcNow();
        await using var transaction = await db.BeginTransactionAsync(cancellationToken);
        var author = await GetOrCreateImporterAsync(now, cancellationToken);
        // Media first, so a stored file exists before any published question shows it.
        var mediaCounts = await mediaImporter.WriteAsync(media, author.Id, now, cancellationToken);
        var result = await WriteAsync(file, validated, existing, author, now, cancellationToken);
        var (banks, testSets) = await catalog.WriteAsync(file, now, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return result with { Banks = banks, TestSets = testSets, Media = mediaCounts };
    }

    private List<(ImportedQuestion Source, AuthoredItem Item)> Validate(
        QuestionImportFile file,
        ImportTags tags,
        Dictionary<Guid, Question> existing,
        out List<ImportIssue> issues,
        out List<MediaReference> mediaReferences)
    {
        issues = [];
        mediaReferences = [];
        var validated = new List<(ImportedQuestion, AuthoredItem)>();
        if (file.Questions.Count == 0)
        {
            issues.Add(new ImportIssue("questions", ImportIssueCodes.Required, "The file has no questions."));
        }

        var seenIds = new HashSet<Guid>();
        for (var i = 0; i < file.Questions.Count; i++)
        {
            var question = file.Questions[i];
            var path = $"questions[{i}]";
            var before = issues.Count;

            if (question.Id == Guid.Empty)
            {
                issues.Add(new ImportIssue($"{path}.id", ImportIssueCodes.Required, "Every question needs a fixed id."));
            }
            else if (!seenIds.Add(question.Id))
            {
                issues.Add(new ImportIssue($"{path}.id", ImportIssueCodes.DuplicateId, $"Id {question.Id} is used more than once."));
            }
            else if (existing.TryGetValue(question.Id, out var current) && current.Track != file.Track)
            {
                issues.Add(new ImportIssue($"{path}.id", ImportIssueCodes.TrackMismatch, $"Question {question.Id} already exists on the {current.Track} track."));
            }

            var item = ValidateQuestion(question, path, tags, issues, mediaReferences);
            if (item is not null && issues.Count == before)
            {
                validated.Add((question, item));
            }
        }

        return validated;
    }

    // Returns the item ready to save, or null when the item-type checks could not run.
    private AuthoredItem? ValidateQuestion(
        ImportedQuestion question,
        string path,
        ImportTags tags,
        List<ImportIssue> issues,
        List<MediaReference> mediaReferences)
    {
        if (string.IsNullOrWhiteSpace(question.Stem))
        {
            issues.Add(new ImportIssue($"{path}.stem", ImportIssueCodes.Required, "The stem is empty."));
        }

        if (string.IsNullOrWhiteSpace(question.EducationalObjective))
        {
            issues.Add(new ImportIssue($"{path}.educationalObjective", ImportIssueCodes.Required, "The educational objective is empty."));
        }

        if (question.TimeAllotmentSec <= 0)
        {
            issues.Add(new ImportIssue($"{path}.timeAllotmentSec", ImportIssueCodes.InvalidValue, "The time allotment must be more than 0 seconds."));
        }

        var exhibits = question.Exhibits ?? JsonSerializer.SerializeToElement(Array.Empty<object>());
        var exhibitCheck = ExhibitRules.Check(exhibits);
        issues.AddRange(exhibitCheck.Issues.Select(issue => new ImportIssue($"{path}.{issue.Path}", issue.Code, issue.Message)));
        mediaReferences.AddRange(exhibitCheck.Media.Concat(ExhibitRules.MediaInContent(question.Content))
            .Select(reference => reference with { Path = $"{path}.{reference.Path}" }));

        if (!tags.LeafIdsByCode.TryGetValue(question.ClientNeeds, out var clientNeedsId))
        {
            issues.Add(new ImportIssue($"{path}.clientNeeds", ImportIssueCodes.UnknownTag, $"\"{question.ClientNeeds}\" is not a Client Needs subcategory of this track."));
        }

        if (!tags.BodySystemIdsBySlug.TryGetValue(question.BodySystem, out var bodySystemId))
        {
            issues.Add(new ImportIssue($"{path}.bodySystem", ImportIssueCodes.UnknownTag, $"\"{question.BodySystem}\" is not a body system."));
        }

        if (!itemTypes.TryGet(question.ItemType, out var module))
        {
            issues.Add(new ImportIssue($"{path}.itemType", ImportIssueCodes.UnsupportedItemType, $"Item type {question.ItemType} is not supported yet."));
            return null;
        }

        Rationale? rationale;
        try
        {
            rationale = question.Rationale.Deserialize<Rationale>(ItemJson.Options);
        }
        catch (JsonException ex)
        {
            rationale = null;
            issues.Add(new ImportIssue($"{path}.rationale", ValidationIssueCodes.InvalidShape, $"The rationale does not have the expected fields: {ex.Message}"));
        }

        if (rationale is null)
        {
            return null;
        }

        var itemIssues = module.ValidateAuthoring(question.Content, question.Key, rationale);
        issues.AddRange(itemIssues.Select(issue => new ImportIssue($"{path}.{issue.Path}", issue.Code, issue.Message)));
        if (itemIssues.Count > 0)
        {
            return null;
        }

        return new AuthoredItem(
            question.ItemType,
            question.Stem,
            question.Content,
            question.Key,
            question.Rationale,
            question.EducationalObjective,
            exhibits,
            clientNeedsId,
            bodySystemId,
            question.CjmmSkill,
            question.Difficulty,
            question.TimeAllotmentSec,
            (short)module.MaxPoints(question.Content, question.Key));
    }

    private async Task<ImportResult> WriteAsync(
        QuestionImportFile file,
        List<(ImportedQuestion Source, AuthoredItem Item)> questions,
        Dictionary<Guid, Question> existing,
        User author,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var currentVersions = await LoadCurrentVersionsAsync(existing.Values, cancellationToken);
        var latestVersionNumbers = await db.QuestionVersions
            .Where(v => existing.Keys.Contains(v.QuestionId))
            .GroupBy(v => v.QuestionId)
            .Select(g => new { QuestionId = g.Key, Latest = g.Max(v => v.Version) })
            .ToDictionaryAsync(x => x.QuestionId, x => x.Latest, cancellationToken);

        int created = 0, updated = 0, unchanged = 0;
        var toPublish = new List<(Question Question, QuestionVersion Version, QuestionVersion? Previous)>();
        foreach (var (source, item) in questions)
        {
            if (!existing.TryGetValue(source.Id, out var question))
            {
                question = Question.Create(source.Id, file.Track, source.AccessTier, author.Id, now);
                db.Questions.Add(question);
                toPublish.Add((question, AddVersion(question, 1, item, author, now), null));
                created++;
                continue;
            }

            var current = question.CurrentVersionId is { } currentId ? currentVersions[currentId] : null;
            if (current is not null && current.Matches(item))
            {
                if (question.AccessTier == source.AccessTier)
                {
                    unchanged++;
                    continue;
                }

                question.ChangeAccessTier(source.AccessTier);
                updated++;
                continue;
            }

            question.ChangeAccessTier(source.AccessTier);
            var number = latestVersionNumbers.GetValueOrDefault(question.Id) + 1;
            toPublish.Add((question, AddVersion(question, number, item, author, now), current));
            updated++;
        }

        // Two saves, because questions and question_versions reference each other: the new rows
        // must exist before a question can point at its current version (see QuestionConfiguration).
        await db.SaveChangesAsync(cancellationToken);
        foreach (var (question, version, previous) in toPublish)
        {
            question.Publish(version, previous, now);
        }

        await db.SaveChangesAsync(cancellationToken);
        return new ImportResult(created, updated, unchanged, []);
    }

    private QuestionVersion AddVersion(Question question, int number, AuthoredItem item, User author, DateTimeOffset now)
    {
        var version = QuestionVersion.CreateImported(question.Id, number, item, author.Id, now);
        db.QuestionVersions.Add(version);
        return version;
    }

    private async Task<Dictionary<Guid, QuestionVersion>> LoadCurrentVersionsAsync(
        IEnumerable<Question> questions,
        CancellationToken cancellationToken)
    {
        var currentIds = questions.Select(q => q.CurrentVersionId).OfType<Guid>().ToList();
        return await db.QuestionVersions
            .Where(v => currentIds.Contains(v.Id))
            .ToDictionaryAsync(v => v.Id, cancellationToken);
    }

    private async Task<User> GetOrCreateImporterAsync(DateTimeOffset now, CancellationToken cancellationToken)
    {
        var importer = await db.Users.SingleOrDefaultAsync(u => u.Email == User.ContentImporterEmail, cancellationToken);
        if (importer is not null)
        {
            return importer;
        }

        importer = User.CreateContentImporter(now);
        db.Users.Add(importer);
        return importer;
    }

    private async Task<ImportTags> LoadTagsAsync(ExamTrack track, CancellationToken cancellationToken)
    {
        var categories = await db.ClientNeedsCategories.AsNoTracking()
            .Where(c => c.Track == track)
            .ToListAsync(cancellationToken);
        var parentIds = categories.Select(c => c.ParentId).OfType<short>().ToHashSet();

        // Questions are always tagged to a leaf (database.md section 2.3).
        var leaves = categories.Where(c => !parentIds.Contains(c.Id)).ToDictionary(c => c.Code, c => c.Id, StringComparer.Ordinal);
        var bodySystems = await db.BodySystems.AsNoTracking()
            .ToDictionaryAsync(b => b.Slug, b => b.Id, StringComparer.Ordinal, cancellationToken);
        return new ImportTags(leaves, bodySystems);
    }

    private sealed record ImportTags(
        IReadOnlyDictionary<string, short> LeafIdsByCode,
        IReadOnlyDictionary<string, short> BodySystemIdsBySlug);
}
