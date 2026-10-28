using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Domain.Content;
using Nclex.Domain.Entities;

namespace Nclex.Application.Content.ImportQuestions;

/// <summary>
/// The banks and test sets part of an import (F06). <see cref="ImportQuestionsHandler"/> runs it
/// after the questions, inside the same transaction, so a file is still all or nothing.
/// </summary>
public sealed class CatalogImporter(IAppDbContext db)
{
    /// <summary>
    /// Adds every problem with the file's banks and sets to <paramref name="issues"/>. Questions may
    /// come from the file itself or be already published on the file's track. A set keeps each case
    /// study it uses whole, as <paramref name="cases"/> will be after the import.
    /// </summary>
    public async Task ValidateAsync(QuestionImportFile file, CaseMembership cases, List<ImportIssue> issues, CancellationToken cancellationToken)
    {
        var banks = file.Banks ?? [];
        var sets = file.TestSets ?? [];
        var known = await LoadUsableQuestionIdsAsync(file, cancellationToken);

        var bankIds = banks.Select(b => b.Id).ToList();
        var existingBanks = await db.QuestionBanks.AsNoTracking()
            .Where(b => bankIds.Contains(b.Id))
            .ToDictionaryAsync(b => b.Id, b => b.Track, cancellationToken);
        var seenBankIds = new HashSet<Guid>();
        for (var i = 0; i < banks.Count; i++)
        {
            var bank = banks[i];
            var path = $"banks[{i}]";
            CheckId(bank.Id, path, "Bank", seenBankIds, existingBanks.TryGetValue(bank.Id, out var track) ? track : null, file.Track, issues);
            if (string.IsNullOrWhiteSpace(bank.Name))
            {
                issues.Add(new ImportIssue($"{path}.name", ImportIssueCodes.Required, "The bank needs a name."));
            }

            CheckQuestionIds(bank.QuestionIds, path, known, issues);
        }

        var setIds = sets.Select(s => s.Id).ToList();
        var existingSets = await db.TestSets.AsNoTracking()
            .Where(s => setIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, s => new { s.Track, s.Kind }, cancellationToken);
        var seenSetIds = new HashSet<Guid>();
        for (var i = 0; i < sets.Count; i++)
        {
            var set = sets[i];
            var path = $"testSets[{i}]";
            var existing = existingSets.GetValueOrDefault(set.Id);
            CheckId(set.Id, path, "Set", seenSetIds, existing?.Track, file.Track, issues);
            if (existing is not null && existing.Kind != TestSetKind.Prebuilt)
            {
                issues.Add(new ImportIssue($"{path}.id", ImportIssueCodes.InvalidValue, $"Set {set.Id} exists and is not a curated set."));
            }

            CheckSettings(set, path, issues);
            CheckQuestionIds(set.QuestionIds, path, known, issues);
            CheckCaseStudies(set.QuestionIds, path, cases, issues);
        }
    }

    /// <summary>Creates or updates the file's banks and sets. Call only after a clean <see cref="ValidateAsync"/>.</summary>
    public async Task<(ImportCounts Banks, ImportCounts TestSets)> WriteAsync(
        QuestionImportFile file,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var banks = await WriteBanksAsync(file, now, cancellationToken);
        var sets = await WriteTestSetsAsync(file, now, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return (banks, sets);
    }

    private async Task<ImportCounts> WriteBanksAsync(QuestionImportFile file, DateTimeOffset now, CancellationToken cancellationToken)
    {
        int created = 0, updated = 0, unchanged = 0;
        foreach (var source in file.Banks ?? [])
        {
            var bank = await db.QuestionBanks.SingleOrDefaultAsync(b => b.Id == source.Id, cancellationToken);
            if (bank is null)
            {
                db.QuestionBanks.Add(QuestionBank.CreatePublished(source.Id, file.Track, source.Name, source.Description, source.AccessTier, now));
                db.QuestionBankItems.AddRange(source.QuestionIds.Select(q => QuestionBankItem.Create(source.Id, q)));
                created++;
                continue;
            }

            var currentIds = await db.QuestionBankItems.Where(i => i.BankId == bank.Id).Select(i => i.QuestionId).ToListAsync(cancellationToken);
            var sameItems = currentIds.ToHashSet().SetEquals(source.QuestionIds);
            if (sameItems && bank.HasDetails(source.Name, source.Description, source.AccessTier))
            {
                unchanged++;
                continue;
            }

            bank.ChangeDetails(source.Name, source.Description, source.AccessTier);
            if (!sameItems)
            {
                await db.QuestionBankItems.Where(i => i.BankId == bank.Id).ExecuteDeleteAsync(cancellationToken);
                db.QuestionBankItems.AddRange(source.QuestionIds.Select(q => QuestionBankItem.Create(bank.Id, q)));
            }

            updated++;
        }

        return new ImportCounts(created, updated, unchanged);
    }

    private async Task<ImportCounts> WriteTestSetsAsync(QuestionImportFile file, DateTimeOffset now, CancellationToken cancellationToken)
    {
        int created = 0, updated = 0, unchanged = 0;
        foreach (var source in file.TestSets ?? [])
        {
            var settings = source.ToSettings();
            var set = await db.TestSets.SingleOrDefaultAsync(s => s.Id == source.Id, cancellationToken);
            if (set is null)
            {
                db.TestSets.Add(TestSet.CreatePrebuilt(source.Id, file.Track, settings, source.QuestionIds.Count, now));
                db.TestSetItems.AddRange(TestSetItem.ForQuestions(source.Id, source.QuestionIds));
                created++;
                continue;
            }

            var currentIds = await db.TestSetItems.Where(i => i.TestSetId == set.Id).OrderBy(i => i.Position)
                .Select(i => i.QuestionId).ToListAsync(cancellationToken);
            // Order matters for sets: it is the order items are served in.
            var sameItems = currentIds.SequenceEqual(source.QuestionIds);
            if (sameItems && set.Settings == settings)
            {
                unchanged++;
                continue;
            }

            set.Apply(settings, source.QuestionIds.Count);
            if (!sameItems)
            {
                // Deleted straight away, because the new rows reuse the same (set, position) keys.
                await db.TestSetItems.Where(i => i.TestSetId == set.Id).ExecuteDeleteAsync(cancellationToken);
                db.TestSetItems.AddRange(TestSetItem.ForQuestions(set.Id, source.QuestionIds));
            }

            updated++;
        }

        return new ImportCounts(created, updated, unchanged);
    }

    // Questions a bank or set may use: the file's own, plus active published ones already on the track.
    private async Task<HashSet<Guid>> LoadUsableQuestionIdsAsync(QuestionImportFile file, CancellationToken cancellationToken)
    {
        var referenced = (file.Banks ?? []).SelectMany(b => b.QuestionIds ?? [])
            .Concat((file.TestSets ?? []).SelectMany(s => s.QuestionIds ?? []))
            .Distinct()
            .ToList();
        var published = await db.Questions.AsNoTracking()
            .Where(q => referenced.Contains(q.Id)
                && q.Track == file.Track
                && q.Status == QuestionStatus.Active
                && q.CurrentVersionId != null)
            .Select(q => q.Id)
            .ToListAsync(cancellationToken);
        return [.. published, .. file.Questions.Select(q => q.Id)];
    }

    private static void CheckId(Guid id, string path, string noun, HashSet<Guid> seen, ExamTrack? existingTrack, ExamTrack fileTrack, List<ImportIssue> issues)
    {
        if (id == Guid.Empty)
        {
            issues.Add(new ImportIssue($"{path}.id", ImportIssueCodes.Required, $"Every {noun.ToLowerInvariant()} needs a fixed id."));
        }
        else if (!seen.Add(id))
        {
            issues.Add(new ImportIssue($"{path}.id", ImportIssueCodes.DuplicateId, $"Id {id} is used more than once."));
        }
        else if (existingTrack is { } track && track != fileTrack)
        {
            issues.Add(new ImportIssue($"{path}.id", ImportIssueCodes.TrackMismatch, $"{noun} {id} already exists on the {track} track."));
        }
    }

    private static void CheckQuestionIds(IReadOnlyList<Guid>? questionIds, string path, HashSet<Guid> known, List<ImportIssue> issues)
    {
        if (questionIds is null || questionIds.Count == 0)
        {
            issues.Add(new ImportIssue($"{path}.questionIds", ImportIssueCodes.Required, "List at least one question."));
            return;
        }

        var seen = new HashSet<Guid>();
        for (var j = 0; j < questionIds.Count; j++)
        {
            var id = questionIds[j];
            if (!seen.Add(id))
            {
                issues.Add(new ImportIssue($"{path}.questionIds[{j}]", ImportIssueCodes.DuplicateId, $"Question {id} is listed more than once."));
            }
            else if (!known.Contains(id))
            {
                issues.Add(new ImportIssue($"{path}.questionIds[{j}]", ImportIssueCodes.UnknownQuestion, $"Question {id} is not in this file or published on this track."));
            }
        }
    }

    // Banks need no such check: the custom builder takes whole cases from a pool (QuestionPool).
    private static void CheckCaseStudies(IReadOnlyList<Guid>? questionIds, string path, CaseMembership cases, List<ImportIssue> issues)
    {
        foreach (var (index, message) in CaseStudyRules.CheckSetOrder(questionIds ?? [], cases.PlacementOf, cases.SizeOf))
        {
            issues.Add(new ImportIssue($"{path}.questionIds[{index}]", ImportIssueCodes.CaseStudySplit, message));
        }
    }

    private static void CheckSettings(ImportedTestSet set, string path, List<ImportIssue> issues)
    {
        if (string.IsNullOrWhiteSpace(set.Title))
        {
            issues.Add(new ImportIssue($"{path}.title", ImportIssueCodes.Required, "The set needs a title."));
        }

        // CAT sets are generated, not curated (architecture.md 5.8).
        if (set.Mode is not (TestMode.Practice or TestMode.Challenge))
        {
            issues.Add(new ImportIssue($"{path}.mode", ImportIssueCodes.InvalidValue, "A curated set is practice or challenge."));
        }

        if (!set.ToSettings().HasConsistentTiming)
        {
            issues.Add(new ImportIssue($"{path}.timed", ImportIssueCodes.InvalidValue, "A timed set needs perQuestionSec or timeBudgetSec, and an untimed set must have neither."));
        }

        CheckPositive(set.PerQuestionSec, $"{path}.perQuestionSec", issues);
        CheckPositive(set.TimeBudgetSec, $"{path}.timeBudgetSec", issues);
        CheckPositive(set.PausePolicy?.MaxPauses, $"{path}.pausePolicy.maxPauses", issues);
        CheckPositive(set.PausePolicy?.MaxPauseSec, $"{path}.pausePolicy.maxPauseSec", issues);

        if (set.PassThreshold is { } threshold && (threshold <= 0 || threshold > 1))
        {
            issues.Add(new ImportIssue($"{path}.passThreshold", ImportIssueCodes.InvalidValue, "The pass threshold is a share of points, above 0 and at most 1."));
        }

        if (set.IsSample && set.QuestionIds is { } ids && ids.Count != TestSet.SampleQuestionCount)
        {
            issues.Add(new ImportIssue($"{path}.questionIds", ImportIssueCodes.InvalidValue, $"A sample set has exactly {TestSet.SampleQuestionCount} questions."));
        }
    }

    private static void CheckPositive(int? value, string path, List<ImportIssue> issues)
    {
        if (value is <= 0)
        {
            issues.Add(new ImportIssue(path, ImportIssueCodes.InvalidValue, "The value must be more than 0."));
        }
    }
}
