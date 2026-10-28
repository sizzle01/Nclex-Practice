using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Domain.Content;
using Nclex.Domain.Entities;

namespace Nclex.Application.Content.ImportQuestions;

/// <summary>
/// The case-study part of an import (F11): the file's case studies, and which questions sit in
/// which case. A case's questions may come from this file or be already published, so the checks
/// look at the case as it will be after the import.
/// </summary>
public sealed class CaseStudyImporter(IAppDbContext db)
{
    /// <summary>
    /// Adds every problem with the file's case studies and the questions' case fields to
    /// <paramref name="issues"/>, and the media the scenarios show to <paramref name="mediaReferences"/>.
    /// Returns where each question will sit, for the test-set checks.
    /// </summary>
    public async Task<CaseMembership> ValidateAsync(
        QuestionImportFile file,
        List<ImportIssue> issues,
        List<MediaReference> mediaReferences,
        CancellationToken cancellationToken)
    {
        var tabIds = await CheckCaseStudiesAsync(file, issues, mediaReferences, cancellationToken);
        var placements = CheckPlacements(file, tabIds, issues);
        return await CheckPositionsAsync(file, placements, issues, cancellationToken);
    }

    /// <summary>Creates or updates the file's case studies. Runs before the questions, which refer to them.</summary>
    public async Task<ImportCounts> WriteAsync(QuestionImportFile file, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var cases = file.CaseStudies ?? [];
        var ids = cases.Select(c => c.Id).ToList();
        var existing = await db.CaseStudies.Where(c => ids.Contains(c.Id)).ToDictionaryAsync(c => c.Id, cancellationToken);
        int created = 0, updated = 0, unchanged = 0;
        foreach (var source in cases)
        {
            var title = source.Title.Trim();
            if (!existing.TryGetValue(source.Id, out var caseStudy))
            {
                db.CaseStudies.Add(CaseStudy.Create(source.Id, file.Track, title, source.Scenario, now));
                created++;
            }
            else if (caseStudy.Update(title, source.Scenario))
            {
                updated++;
            }
            else
            {
                unchanged++;
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        return new ImportCounts(created, updated, unchanged);
    }

    // Checks the file's own case studies; returns the tab ids of every case a question may join
    // (the file's, and those already imported on the track that the questions name).
    private async Task<Dictionary<Guid, IReadOnlySet<string>>> CheckCaseStudiesAsync(
        QuestionImportFile file,
        List<ImportIssue> issues,
        List<MediaReference> mediaReferences,
        CancellationToken cancellationToken)
    {
        var cases = file.CaseStudies ?? [];
        var named = cases.Select(c => c.Id)
            .Concat(file.Questions.Select(q => q.CaseStudyId).OfType<Guid>())
            .Distinct()
            .ToList();
        var stored = await db.CaseStudies.AsNoTracking()
            .Where(c => named.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, cancellationToken);

        var tabIds = stored.Values.Where(c => c.Track == file.Track)
            .ToDictionary(c => c.Id, c => CaseStudyRules.TabIds(c.Scenario.RootElement));
        var seen = new HashSet<Guid>();
        for (var i = 0; i < cases.Count; i++)
        {
            var caseStudy = cases[i];
            var path = $"caseStudies[{i}]";
            if (caseStudy.Id == Guid.Empty)
            {
                issues.Add(new ImportIssue($"{path}.id", ImportIssueCodes.Required, "Every case study needs a fixed id."));
            }
            else if (!seen.Add(caseStudy.Id))
            {
                issues.Add(new ImportIssue($"{path}.id", ImportIssueCodes.DuplicateId, $"Id {caseStudy.Id} is used more than once."));
            }
            else if (stored.TryGetValue(caseStudy.Id, out var current) && current.Track != file.Track)
            {
                issues.Add(new ImportIssue($"{path}.id", ImportIssueCodes.TrackMismatch, $"Case study {caseStudy.Id} already exists on the {current.Track} track."));
            }
            else
            {
                tabIds[caseStudy.Id] = CaseStudyRules.TabIds(caseStudy.Scenario);
            }

            if (string.IsNullOrWhiteSpace(caseStudy.Title))
            {
                issues.Add(new ImportIssue($"{path}.title", ImportIssueCodes.Required, "The case study needs a title."));
            }

            var check = CaseStudyRules.CheckScenario(caseStudy.Scenario);
            issues.AddRange(check.Issues.Select(issue => new ImportIssue($"{path}.{issue.Path}", issue.Code, issue.Message)));
            mediaReferences.AddRange(check.Media.Select(reference => reference with { Path = $"{path}.{reference.Path}" }));
        }

        return tabIds;
    }

    // Checks each question's caseStudyId and casePosition; returns the valid placements by question index.
    private static Dictionary<int, CasePlacement> CheckPlacements(
        QuestionImportFile file,
        Dictionary<Guid, IReadOnlySet<string>> tabIds,
        List<ImportIssue> issues)
    {
        var placements = new Dictionary<int, CasePlacement>();
        for (var i = 0; i < file.Questions.Count; i++)
        {
            var question = file.Questions[i];
            var path = $"questions[{i}]";
            switch (question.CaseStudyId, question.CasePosition)
            {
                case (null, null):
                    continue;
                case (null, _):
                    issues.Add(new ImportIssue($"{path}.caseStudyId", ImportIssueCodes.Required, "A question with a casePosition needs a caseStudyId."));
                    continue;
                case (_, null):
                    issues.Add(new ImportIssue($"{path}.casePosition", ImportIssueCodes.Required, "A question in a case study needs a casePosition."));
                    continue;
            }

            var caseId = question.CaseStudyId.Value;
            var position = question.CasePosition.Value;
            if (position is < 1 or > CaseStudy.MaxItems)
            {
                issues.Add(new ImportIssue($"{path}.casePosition", ImportIssueCodes.InvalidValue, $"The position is 1 to {CaseStudy.MaxItems}."));
                continue;
            }

            if (!tabIds.TryGetValue(caseId, out var tabs))
            {
                issues.Add(new ImportIssue($"{path}.caseStudyId", ImportIssueCodes.UnknownCaseStudy, $"Case study {caseId} is not in this file or imported on this track."));
                continue;
            }

            // The player puts the case's tabs and the item's exhibits in one tab list.
            var exhibitIds = ExhibitIds(question.Exhibits);
            for (var j = 0; j < exhibitIds.Count; j++)
            {
                if (exhibitIds[j] is { } id && tabs.Contains(id))
                {
                    issues.Add(new ImportIssue($"{path}.exhibits[{j}].id", ImportIssueCodes.DuplicateId, $"\"{id}\" is also a tab of the case study; exhibit and tab ids must differ."));
                }
            }

            placements[i] = new CasePlacement(caseId, position);
        }

        return placements;
    }

    // Checks that every case the file touches still has positions 1..n afterwards. A case's members
    // are its questions in this file plus its published questions that the file does not mention.
    private async Task<CaseMembership> CheckPositionsAsync(
        QuestionImportFile file,
        Dictionary<int, CasePlacement> placements,
        List<ImportIssue> issues,
        CancellationToken cancellationToken)
    {
        var fileIds = file.Questions.Select(q => q.Id).ToHashSet();
        var lookup = fileIds.Concat((file.TestSets ?? []).SelectMany(s => s.QuestionIds ?? [])).Distinct().ToList();
        var current = await PlacementsAsync(PublishedInCases().Where(x => lookup.Contains(x.Question.Id)), cancellationToken);

        // Each case to check, with the path to report a problem at.
        var touched = new Dictionary<Guid, string>();
        var cases = file.CaseStudies ?? [];
        for (var i = 0; i < cases.Count; i++)
        {
            touched.TryAdd(cases[i].Id, $"caseStudies[{i}].id");
        }

        for (var i = 0; i < file.Questions.Count; i++)
        {
            if (placements.TryGetValue(i, out var placement))
            {
                touched.TryAdd(placement.CaseStudyId, $"questions[{i}].casePosition");
            }

            // A question that leaves a case can leave a gap behind it.
            if (current.TryGetValue(file.Questions[i].Id, out var before) && before.CaseStudyId != placement?.CaseStudyId)
            {
                touched.TryAdd(before.CaseStudyId, $"questions[{i}].caseStudyId");
            }
        }

        var caseIds = touched.Keys
            .Concat(current.Where(c => !fileIds.Contains(c.Key)).Select(c => c.Value.CaseStudyId))
            .Distinct()
            .ToList();
        var members = await PlacementsAsync(PublishedInCases().Where(x => caseIds.Contains(x.Version.CaseStudyId!.Value)), cancellationToken);

        // The file decides where its own questions sit, including outside any case.
        foreach (var id in fileIds)
        {
            members.Remove(id);
        }

        foreach (var (index, placement) in placements)
        {
            members[file.Questions[index].Id] = placement;
        }

        var membership = new CaseMembership(members);
        foreach (var (caseId, path) in touched)
        {
            var positions = membership.PositionsOf(caseId);
            if (!CaseStudyRules.PositionsAreComplete(positions))
            {
                issues.Add(new ImportIssue(
                    path,
                    ImportIssueCodes.InvalidValue,
                    $"Case study {caseId} would have positions [{string.Join(", ", positions.Order())}]; they must run 1, 2, 3… with no gap or repeat, up to {CaseStudy.MaxItems}."));
            }
        }

        return membership;
    }

    // Active questions whose published version sits in a case study.
    private IQueryable<QuestionInCase> PublishedInCases() =>
        from q in db.Questions.AsNoTracking()
        join v in db.QuestionVersions.AsNoTracking() on q.CurrentVersionId equals v.Id
        where q.Status == QuestionStatus.Active && v.CaseStudyId != null
        select new QuestionInCase { Question = q, Version = v };

    private static async Task<Dictionary<Guid, CasePlacement>> PlacementsAsync(IQueryable<QuestionInCase> query, CancellationToken cancellationToken)
    {
        var rows = await query
            .Select(x => new { x.Question.Id, CaseStudyId = x.Version.CaseStudyId!.Value, Position = x.Version.CasePosition!.Value })
            .ToListAsync(cancellationToken);
        return rows.ToDictionary(r => r.Id, r => new CasePlacement(r.CaseStudyId, r.Position));
    }

    private static List<string?> ExhibitIds(JsonElement? exhibits) =>
        exhibits is { ValueKind: JsonValueKind.Array } array
            ? [.. array.EnumerateArray().Select(e => e.ValueKind == JsonValueKind.Object && e.TryGetProperty("id", out var id) && id.ValueKind == JsonValueKind.String ? id.GetString() : null)]
            : [];

    private sealed class QuestionInCase
    {
        public required Question Question { get; init; }

        public required QuestionVersion Version { get; init; }
    }
}

/// <summary>Where each question will sit in a case study once the import is written.</summary>
public sealed class CaseMembership(IReadOnlyDictionary<Guid, CasePlacement> placements)
{
    /// <summary>A question's place in its case, or null for a standalone question.</summary>
    public CasePlacement? PlacementOf(Guid questionId) => placements.GetValueOrDefault(questionId);

    /// <summary>How many questions a case has.</summary>
    public int SizeOf(Guid caseStudyId) => placements.Values.Count(p => p.CaseStudyId == caseStudyId);

    /// <summary>The positions a case's questions take.</summary>
    public IReadOnlyList<short> PositionsOf(Guid caseStudyId) =>
        [.. placements.Values.Where(p => p.CaseStudyId == caseStudyId).Select(p => p.Position)];
}
