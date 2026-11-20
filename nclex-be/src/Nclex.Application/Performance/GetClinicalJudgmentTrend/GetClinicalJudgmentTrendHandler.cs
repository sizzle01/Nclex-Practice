using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Application.Attempts.Results;
using Nclex.Domain.Entities;
using Nclex.Domain.Grading;

namespace Nclex.Application.Performance.GetClinicalJudgmentTrend;

/// <summary>
/// GET /me/performance/clinical-judgment: each CJMM step's score in the user's last few timed attempts (F25).
/// Timed attempts only, because the pattern being tracked (judgment falling away under time pressure) shows under
/// exam conditions. Reads the per-attempt scores stored at grading, so it never re-grades.
/// </summary>
public sealed class GetClinicalJudgmentTrendHandler(IAppDbContext db, PerformanceViewers viewers, DimensionNames dimensionNames)
{
    /// <summary>As many attempts as the candidate's NCLEX report compared.</summary>
    public const int RecentAttempts = 5;

    public async Task<ClinicalJudgmentTrend> HandleAsync(GetClinicalJudgmentTrendRequest request, CancellationToken cancellationToken)
    {
        var viewer = await viewers.CurrentAsync(cancellationToken);
        var track = request.Track ?? viewer.Track;

        var attempts = await db.Attempts.AsNoTracking()
            .Where(a => a.UserId == viewer.UserId
                && a.Track == track
                && a.Status == AttemptStatus.Submitted
                && a.Settings.Timed)
            .OrderByDescending(a => a.SubmittedAt)
            .Take(RecentAttempts)
            .Join(db.TestSets, a => a.TestSetId, s => s.Id, (a, s) => new TrendAttempt(a.Id, s.Title, a.SubmittedAt!.Value))
            .ToListAsync(cancellationToken);
        attempts.Reverse();

        var ids = attempts.Select(a => a.AttemptId).ToList();
        var scores = await db.AttemptCategoryScores.AsNoTracking()
            .Where(s => ids.Contains(s.AttemptId) && s.Dimension == ScoreDimension.CjmmSkill)
            .ToListAsync(cancellationToken);
        var names = await dimensionNames.ForTrackAsync(track, cancellationToken);

        var steps = ClinicalJudgment.StepOrder.Select(skill =>
        {
            var key = DimensionKeys.For(skill);
            var row = attempts.Select(a => scores.SingleOrDefault(s => s.AttemptId == a.AttemptId && s.DimensionKey == key)?.Pct).ToList();
            return new StepTrend(key, names.Name(ScoreDimension.CjmmSkill, key), row);
        }).ToList();

        return new ClinicalJudgmentTrend(track, attempts, steps);
    }
}
