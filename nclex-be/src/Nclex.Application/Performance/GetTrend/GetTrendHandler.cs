using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Domain.Entities;
using Nclex.Domain.Performance;

namespace Nclex.Application.Performance.GetTrend;

/// <summary>
/// GET /me/performance/trend. Read from the stored results joined to attempts.submitted_at, which
/// the (user_id, submitted_at) index serves, so trends need no table of their own (database.md 2.7).
/// The rows are grouped into the user's local days or weeks in memory (<see cref="TrendSeries"/>).
/// </summary>
public sealed class GetTrendHandler(IAppDbContext db, PerformanceViewers viewers)
{
    public async Task<PerformanceTrend> HandleAsync(GetTrendRequest request, CancellationToken cancellationToken)
    {
        var dimension = request.Dimension ?? PerformanceDimension.Overall;
        var key = dimension == PerformanceDimension.Overall ? PerformanceDimensions.OverallKey : request.Key!;
        var bucket = request.Bucket ?? TrendBucket.Week;
        var viewer = await viewers.CurrentAsync(cancellationToken);
        var track = request.Track ?? viewer.Track;

        var attempts = db.Attempts.AsNoTracking()
            .Where(a => a.UserId == viewer.UserId && a.Track == track && a.Status == AttemptStatus.Submitted);
        if (request.From is { } from)
        {
            var fromUtc = TrendBucketing.StartOfDay(from, viewer.TimeZone);
            attempts = attempts.Where(a => a.SubmittedAt >= fromUtc);
        }

        if (request.To is { } to)
        {
            // Up to the start of the next local day, so the whole of the "to" date counts.
            var endUtc = TrendBucketing.StartOfDay(to.AddDays(1), viewer.TimeZone);
            attempts = attempts.Where(a => a.SubmittedAt < endUtc);
        }

        var samples = await SamplesAsync(attempts, dimension, key, cancellationToken);
        return new PerformanceTrend(track, dimension, key, bucket, TrendSeries.Build(samples, viewer.TimeZone, bucket));
    }

    private async Task<List<TrendSample>> SamplesAsync(
        IQueryable<Attempt> attempts,
        PerformanceDimension dimension,
        string key,
        CancellationToken cancellationToken)
    {
        if (PerformanceDimensions.ToScoreDimension(dimension) is not { } scoreDimension)
        {
            return await (
                    from attempt in attempts
                    join result in db.AttemptResults on attempt.Id equals result.AttemptId
                    select new TrendSample(attempt.SubmittedAt!.Value, result.ItemsTotal, result.PointsEarned, result.MaxPoints, result.TotalTimeMs))
                .ToListAsync(cancellationToken);
        }

        return await (
                from attempt in attempts
                join score in db.AttemptCategoryScores on attempt.Id equals score.AttemptId
                where score.Dimension == scoreDimension && score.DimensionKey == key
                select new TrendSample(attempt.SubmittedAt!.Value, score.Items, score.PointsEarned, score.MaxPoints, score.TimeMs))
            .ToListAsync(cancellationToken);
    }
}
