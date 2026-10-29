using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Domain.Content;
using Nclex.Domain.Entities;
using Nclex.Domain.ItemTypes;

namespace Nclex.Application.Admin.Membership;

/// <summary>
/// Checks the questions an admin puts in a curated set or bank, with the importer's rules (F06,
/// F11): each one is on the same track, active and published, listed once, and a set keeps every
/// case study whole and in order. Also lists members for the editor.
/// </summary>
public sealed class QuestionMembership(IAppDbContext db)
{
    /// <summary>Adds a problem for each bad entry; paths are "questionIds[i]".</summary>
    /// <param name="track">The set's or bank's track.</param>
    /// <param name="questionIds">The questions, in order.</param>
    /// <param name="keepCasesWhole">True for sets, whose order is the order items are served.</param>
    /// <param name="issues">Where problems are added.</param>
    /// <param name="cancellationToken">Cancels the database reads.</param>
    public async Task CheckAsync(ExamTrack track, IReadOnlyList<Guid> questionIds, bool keepCasesWhole, List<ValidationIssue> issues, CancellationToken cancellationToken)
    {
        var usable = await db.Questions.AsNoTracking()
            .Where(q => questionIds.Contains(q.Id) && q.Track == track && q.Status == QuestionStatus.Active && q.CurrentVersionId != null)
            .Select(q => q.Id)
            .ToListAsync(cancellationToken);
        var usableIds = usable.ToHashSet();
        var seen = new HashSet<Guid>();
        for (var i = 0; i < questionIds.Count; i++)
        {
            var id = questionIds[i];
            if (!seen.Add(id))
            {
                issues.Add(new ValidationIssue($"questionIds[{i}]", ValidationIssueCodes.DuplicateId, $"Question {id} is listed more than once."));
            }
            else if (!usableIds.Contains(id))
            {
                issues.Add(new ValidationIssue($"questionIds[{i}]", ValidationIssueCodes.UnknownId, $"Question {id} is not a published, active {track} question."));
            }
        }

        if (keepCasesWhole && issues.Count == 0)
        {
            await CheckCaseStudiesAsync(questionIds, issues, cancellationToken);
        }
    }

    /// <summary>The members in the order given, for the editor.</summary>
    public async Task<IReadOnlyList<MemberQuestion>> DescribeAsync(IReadOnlyList<Guid> questionIds, CancellationToken cancellationToken)
    {
        var questions = await db.Questions.AsNoTracking()
            .Where(q => questionIds.Contains(q.Id))
            .ToDictionaryAsync(q => q.Id, cancellationToken);
        var versions = await db.QuestionVersions.AsNoTracking()
            .Where(v => questionIds.Contains(v.QuestionId))
            .Select(v => new { v.Id, v.QuestionId, v.Version, v.Stem, v.ItemType })
            .ToListAsync(cancellationToken);
        var byQuestion = versions.ToLookup(v => v.QuestionId);

        return [.. questionIds.Where(questions.ContainsKey).Select(id =>
        {
            var question = questions[id];
            // Show what candidates get; a question with nothing published shows its latest draft.
            var current = byQuestion[id].FirstOrDefault(v => v.Id == question.CurrentVersionId);
            var shown = current ?? byQuestion[id].MaxBy(v => v.Version)!;
            return new MemberQuestion(id, shown.Stem, shown.ItemType, question.Status, current?.Version);
        })];
    }

    // A case's size is how many of its questions candidates can be served now.
    private async Task CheckCaseStudiesAsync(IReadOnlyList<Guid> questionIds, List<ValidationIssue> issues, CancellationToken cancellationToken)
    {
        var placements = await (
                from q in db.Questions.AsNoTracking()
                join v in db.QuestionVersions.AsNoTracking() on q.CurrentVersionId equals v.Id
                where questionIds.Contains(q.Id) && v.CaseStudyId != null
                select new { q.Id, CaseStudyId = v.CaseStudyId!.Value, Position = v.CasePosition!.Value })
            .ToDictionaryAsync(p => p.Id, p => new CasePlacement(p.CaseStudyId, p.Position), cancellationToken);
        if (placements.Count == 0)
        {
            return;
        }

        var caseIds = placements.Values.Select(p => p.CaseStudyId).Distinct().ToList();
        var sizes = await (
                from q in db.Questions.AsNoTracking()
                join v in db.QuestionVersions.AsNoTracking() on q.CurrentVersionId equals v.Id
                where q.Status == QuestionStatus.Active && v.CaseStudyId != null && caseIds.Contains(v.CaseStudyId!.Value)
                group v by v.CaseStudyId into g
                select new { CaseStudyId = g.Key!.Value, Size = g.Count() })
            .ToDictionaryAsync(g => g.CaseStudyId, g => g.Size, cancellationToken);

        foreach (var (index, message) in CaseStudyRules.CheckSetOrder(questionIds, placements.GetValueOrDefault, caseId => sizes.GetValueOrDefault(caseId)))
        {
            issues.Add(new ValidationIssue($"questionIds[{index}]", ValidationIssueCodes.Mismatch, message));
        }
    }
}
