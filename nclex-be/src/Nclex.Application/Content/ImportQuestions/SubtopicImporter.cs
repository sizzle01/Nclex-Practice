using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Domain.Entities;
using Nclex.Domain.Recommendations;

namespace Nclex.Application.Content.ImportQuestions;

/// <summary>
/// The subtopic part of an import (F17). <see cref="ImportQuestionsHandler"/> runs it inside the same
/// transaction, so a file is still all or nothing. These checks are the automated half of tag review
/// (docs/features/F17): every tag is real and in its question's own category, and the list has no
/// unused entries or two names for one topic.
/// </summary>
public sealed class SubtopicImporter(IAppDbContext db)
{
    /// <summary>Adds every problem with the file's subtopics and question tags to <paramref name="issues"/>.</summary>
    public async Task ValidateAsync(QuestionImportFile file, List<ImportIssue> issues, CancellationToken cancellationToken)
    {
        var declared = file.Subtopics ?? [];
        if (declared.Count == 0 && file.Questions.All(q => q.Subtopics is null))
        {
            return;
        }

        var categories = await LoadLeavesAsync(file.Track, cancellationToken);
        var stored = await LoadStoredAsync(file.Track, cancellationToken);
        CheckDeclared(declared, categories, stored, issues);

        var declaredKeys = declared.Select(s => (s.ClientNeeds, s.Slug)).ToHashSet();
        // Listing a deactivated subtopic doesn't bring it back; that is a decision for the console.
        var inactive = stored.Where(s => !s.Subtopic.IsActive).Select(s => (s.Code, s.Subtopic.Slug)).ToHashSet();
        var usable = stored.Select(s => (s.Code, s.Subtopic.Slug)).Concat(declaredKeys).Where(key => !inactive.Contains(key)).ToHashSet();
        var used = new HashSet<(string, string)>();
        for (var i = 0; i < file.Questions.Count; i++)
        {
            CheckQuestion(file.Questions[i], $"questions[{i}].subtopics", declared.Count > 0, usable, used, issues);
        }

        for (var i = 0; i < declared.Count; i++)
        {
            if (!used.Contains((declared[i].ClientNeeds, declared[i].Slug)))
            {
                issues.Add(new ImportIssue($"subtopics[{i}].slug", ImportIssueCodes.UnusedSubtopic, $"No question in this file uses \"{declared[i].Slug}\"."));
            }
        }
    }

    /// <summary>Creates or renames the file's subtopics. Call only after a clean <see cref="ValidateAsync"/>.</summary>
    public async Task<ImportCounts> WriteSubtopicsAsync(QuestionImportFile file, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var declared = file.Subtopics ?? [];
        if (declared.Count == 0)
        {
            return ImportCounts.None;
        }

        var categories = await LoadLeavesAsync(file.Track, cancellationToken);
        var ids = categories.Values.ToList();
        var existing = await db.Subtopics.Where(s => ids.Contains(s.ClientNeedsId)).ToListAsync(cancellationToken);
        int created = 0, updated = 0, unchanged = 0;
        for (var i = 0; i < declared.Count; i++)
        {
            var source = declared[i];
            var sortOrder = (short)i;
            var subtopic = existing.SingleOrDefault(s => s.ClientNeedsId == categories[source.ClientNeeds] && s.Slug == source.Slug);
            if (subtopic is null)
            {
                db.Subtopics.Add(Subtopic.Create(categories[source.ClientNeeds], source.Slug, source.Name, sortOrder, now));
                created++;
            }
            else if (subtopic.Name == source.Name && subtopic.SortOrder == sortOrder)
            {
                unchanged++;
            }
            else
            {
                subtopic.ChangeDetails(source.Name, sortOrder);
                updated++;
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        return new ImportCounts(created, updated, unchanged);
    }

    /// <summary>Replaces the tags of each question that lists some; call after the questions are saved.</summary>
    public async Task WriteTagsAsync(QuestionImportFile file, CancellationToken cancellationToken)
    {
        var tagged = file.Questions.Where(q => q.Subtopics is not null).ToList();
        if (tagged.Count == 0)
        {
            return;
        }

        var categories = await LoadLeavesAsync(file.Track, cancellationToken);
        var ids = categories.Values.ToList();
        var subtopics = await db.Subtopics.AsNoTracking()
            .Where(s => ids.Contains(s.ClientNeedsId))
            .ToDictionaryAsync(s => (s.ClientNeedsId, s.Slug), s => s.Id, cancellationToken);
        var questionIds = tagged.Select(q => q.Id).ToList();
        var current = await db.QuestionSubtopics
            .Where(t => questionIds.Contains(t.QuestionId))
            .ToListAsync(cancellationToken);
        db.QuestionSubtopics.RemoveRange(current);
        await db.SaveChangesAsync(cancellationToken);

        foreach (var question in tagged)
        {
            var clientNeedsId = categories[question.ClientNeeds];
            db.QuestionSubtopics.AddRange(question.Subtopics!.Select(slug => QuestionSubtopic.Create(question.Id, subtopics[(clientNeedsId, slug)])));
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static void CheckDeclared(
        IReadOnlyList<ImportedSubtopic> declared,
        Dictionary<string, short> categories,
        List<StoredSubtopic> stored,
        List<ImportIssue> issues)
    {
        var declaredKeys = declared.Select(s => (s.ClientNeeds, s.Slug)).ToHashSet();
        // A declared entry replaces its stored name, so only the other stored names can clash.
        var namesInUse = stored
            .Where(s => !declaredKeys.Contains((s.Code, s.Subtopic.Slug)))
            .Select(s => SubtopicNames.ComparisonKey(s.Subtopic.Name))
            .ToHashSet();
        var seen = new HashSet<(string, string)>();
        for (var i = 0; i < declared.Count; i++)
        {
            var subtopic = declared[i];
            var path = $"subtopics[{i}]";
            if (!categories.ContainsKey(subtopic.ClientNeeds))
            {
                issues.Add(new ImportIssue($"{path}.clientNeeds", ImportIssueCodes.UnknownTag, $"\"{subtopic.ClientNeeds}\" is not a Client Needs subcategory of this track."));
            }

            if (!SubtopicNames.IsSlug(subtopic.Slug))
            {
                issues.Add(new ImportIssue($"{path}.slug", ImportIssueCodes.InvalidValue, "A slug uses lower-case letters and digits with single hyphens."));
            }
            else if (!seen.Add((subtopic.ClientNeeds, subtopic.Slug)))
            {
                issues.Add(new ImportIssue($"{path}.slug", ImportIssueCodes.DuplicateId, $"\"{subtopic.Slug}\" is listed more than once."));
            }

            if (string.IsNullOrWhiteSpace(subtopic.Name) || subtopic.Name.Length > SubtopicNames.MaxNameLength)
            {
                issues.Add(new ImportIssue($"{path}.name", ImportIssueCodes.InvalidValue, $"The name needs 1 to {SubtopicNames.MaxNameLength} characters."));
            }
            else if (!namesInUse.Add(SubtopicNames.ComparisonKey(subtopic.Name)))
            {
                issues.Add(new ImportIssue($"{path}.name", ImportIssueCodes.DuplicateSubtopic, $"\"{subtopic.Name}\" has the same meaning as another subtopic on this track."));
            }
        }
    }

    private static void CheckQuestion(
        ImportedQuestion question,
        string path,
        bool required,
        HashSet<(string, string)> usable,
        HashSet<(string, string)> used,
        List<ImportIssue> issues)
    {
        if (question.Subtopics is not { } slugs)
        {
            if (required)
            {
                issues.Add(new ImportIssue(path, ImportIssueCodes.Required, "This file lists subtopics, so every question needs at least one."));
            }

            return;
        }

        if (slugs.Count is < SubtopicNames.MinPerQuestion or > SubtopicNames.MaxPerQuestion || slugs.Distinct().Count() != slugs.Count)
        {
            issues.Add(new ImportIssue(path, ImportIssueCodes.InvalidValue, $"A question needs {SubtopicNames.MinPerQuestion} to {SubtopicNames.MaxPerQuestion} different subtopics."));
        }

        foreach (var slug in slugs)
        {
            if (usable.Contains((question.ClientNeeds, slug)))
            {
                used.Add((question.ClientNeeds, slug));
            }
            else
            {
                issues.Add(new ImportIssue(path, ImportIssueCodes.UnknownTag, $"\"{slug}\" is not an active subtopic of {question.ClientNeeds}."));
            }
        }
    }

    // Tags attach to leaves only, as questions do (database.md section 2.3).
    private async Task<Dictionary<string, short>> LoadLeavesAsync(ExamTrack track, CancellationToken cancellationToken) =>
        await db.ClientNeedsCategories.AsNoTracking()
            .Where(c => c.Track == track && !db.ClientNeedsCategories.Any(child => child.ParentId == c.Id))
            .ToDictionaryAsync(c => c.Code, c => c.Id, StringComparer.Ordinal, cancellationToken);

    private async Task<List<StoredSubtopic>> LoadStoredAsync(ExamTrack track, CancellationToken cancellationToken) =>
        await (
            from subtopic in db.Subtopics.AsNoTracking()
            join category in db.ClientNeedsCategories.AsNoTracking() on subtopic.ClientNeedsId equals category.Id
            where category.Track == track
            select new StoredSubtopic(category.Code, subtopic))
            .ToListAsync(cancellationToken);

    private sealed record StoredSubtopic(string Code, Subtopic Subtopic);
}
