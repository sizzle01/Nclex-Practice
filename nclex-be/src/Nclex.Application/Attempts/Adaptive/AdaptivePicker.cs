using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Domain.Builder;
using Nclex.Domain.Cat;
using Nclex.Domain.Entities;

namespace Nclex.Application.Attempts.Adaptive;

/// <summary>
/// Loads what the adaptive exam chooses from and asks <see cref="ItemSelector"/> for the next
/// question (architecture.md 5.8). The pool is every published standalone question of the track
/// that the candidate's plan allows, so newly published questions join without touching the set.
/// Case-study items are left out for now: a case study is six linked items, which the one-at-a-time
/// selection cannot serve as a unit.
/// </summary>
public class AdaptivePicker(IAppDbContext db, Random random)
{
    /// <summary>
    /// The next question for an attempt in <paramref name="state"/> (null before the first), or
    /// null when nothing is left to give.
    /// </summary>
    public async Task<CatCandidate?> NextAsync(ExamTrack track, PlanTier plan, CatState? state, CancellationToken cancellationToken)
    {
        var given = state?.GivenQuestionIds ?? new HashSet<Guid>();
        var pool = await PoolAsync(track, plan, given, cancellationToken);
        var testPlan = await TestPlanAsync(track, cancellationToken);
        return ItemSelector.Choose(
            pool,
            state?.Theta ?? AbilityEstimate.Prior.Theta,
            state?.ContentCounts ?? new Dictionary<short, int>(),
            testPlan,
            random);
    }

    /// <summary>
    /// The questions the exam may still give. Virtual only so integration tests on the shared
    /// database can keep an exam to their own questions; the server always uses this one.
    /// </summary>
    protected virtual async Task<List<CatCandidate>> PoolAsync(
        ExamTrack track,
        PlanTier plan,
        IReadOnlySet<Guid> given,
        CancellationToken cancellationToken)
    {
        var includePremium = plan == PlanTier.Premium;
        var givenIds = given.ToList();
        var rows = await (
                from question in db.Questions
                join version in db.QuestionVersions on question.CurrentVersionId equals version.Id
                where question.Track == track
                      && question.Status == QuestionStatus.Active
                      && version.Status == VersionStatus.Published
                      && version.CaseStudyId == null
                      && (question.AccessTier == PlanTier.Free || includePremium)
                      && !givenIds.Contains(question.Id)
                select new { QuestionId = question.Id, VersionId = version.Id, version.IrtB, version.Difficulty, version.ClientNeedsId })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return [.. rows.Select(r => new CatCandidate(r.QuestionId, r.VersionId, RaschModel.DifficultyOf(r.IrtB, r.Difficulty), r.ClientNeedsId))];
    }

    /// <summary>The track's Client Needs leaves with the middle of their test-plan range as weight.</summary>
    private async Task<List<LeafWeight>> TestPlanAsync(ExamTrack track, CancellationToken cancellationToken)
    {
        var categories = await db.ClientNeedsCategories.AsNoTracking()
            .Where(c => c.Track == track)
            .Select(c => new { c.Id, c.ParentId, c.WeightMin, c.WeightMax })
            .ToListAsync(cancellationToken);
        var parents = categories.Where(c => c.ParentId is not null).Select(c => c.ParentId!.Value).ToHashSet();
        return [.. categories
            .Where(c => !parents.Contains(c.Id))
            .Select(c => new LeafWeight(c.Id, ((c.WeightMin ?? 0) + (c.WeightMax ?? 0)) / 2))];
    }
}
