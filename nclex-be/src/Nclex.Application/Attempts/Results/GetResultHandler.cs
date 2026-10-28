using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Application.Common;
using Nclex.Domain.Entities;
using Nclex.Domain.Grading;

namespace Nclex.Application.Attempts.Results;

/// <summary>
/// GET /attempts/{id}/result: the score summary and breakdown, read from what grading stored at
/// submit, so the page never re-grades (architecture.md 5.5).
/// </summary>
public sealed class GetResultHandler(IAppDbContext db, AttemptStore store, DimensionNames dimensionNames)
{
    public async Task<AttemptResultView> HandleAsync(Guid attemptId, CancellationToken cancellationToken)
    {
        var attempt = await store.FindOwnedAsync(attemptId, cancellationToken);
        if (attempt.Status != AttemptStatus.Submitted)
        {
            throw new AppException(attempt.Status == AttemptStatus.Abandoned ? AttemptErrors.NotActive : AttemptErrors.NotSubmitted);
        }

        var result = await db.AttemptResults.AsNoTracking().SingleOrDefaultAsync(r => r.AttemptId == attempt.Id, cancellationToken)
            ?? throw new AppException(AttemptErrors.ResultNotFound);
        var scores = await db.AttemptCategoryScores.AsNoTracking()
            .Where(s => s.AttemptId == attempt.Id)
            .ToListAsync(cancellationToken);
        var title = await db.TestSets.AsNoTracking()
            .Where(s => s.Id == attempt.TestSetId)
            .Select(s => s.Title)
            .SingleAsync(cancellationToken);
        var names = await dimensionNames.ForTrackAsync(attempt.Track, cancellationToken);

        var summary = AttemptResultSummary.From(result);
        return new AttemptResultView(
            attempt.Id,
            title,
            attempt.SubmittedAt!.Value,
            attempt.SubmitReason!.Value,
            summary.Score,
            summary.Time,
            summary.Readiness,
            new ResultBreakdown(
                Rows(ScoreDimension.ClientNeeds),
                Rows(ScoreDimension.ClientNeedsParent),
                Rows(ScoreDimension.BodySystem),
                Rows(ScoreDimension.ItemType),
                Rows(ScoreDimension.CjmmSkill),
                Rows(ScoreDimension.Difficulty)),
            [.. WeakestAreas.From(scores).Select(s => new WeakArea(s.Dimension, s.DimensionKey, names.Name(s.Dimension, s.DimensionKey), s.Pct))]);

        IReadOnlyList<CategoryResult> Rows(ScoreDimension dimension) =>
            [.. scores
                .Where(s => s.Dimension == dimension)
                .OrderBy(s => s.DimensionKey, StringComparer.Ordinal)
                .Select(s => new CategoryResult(
                    s.DimensionKey,
                    names.Name(dimension, s.DimensionKey),
                    s.Items,
                    s.ItemsCorrect,
                    s.PointsEarned,
                    s.MaxPoints,
                    s.Pct,
                    (int)(s.TimeMs / s.Items)))];
    }
}
