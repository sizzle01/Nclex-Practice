using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Application.Attempts.Results;
using Nclex.Domain.Performance;

namespace Nclex.Application.Performance.GetSummary;

/// <summary>GET /me/performance/summary: the overall snapshot row plus the weakest categories.</summary>
public sealed class GetSummaryHandler(IAppDbContext db, PerformanceViewers viewers, DimensionNames dimensionNames)
{
    public async Task<PerformanceSummary> HandleAsync(GetSummaryRequest request, CancellationToken cancellationToken)
    {
        var viewer = await viewers.CurrentAsync(cancellationToken);
        var track = request.Track ?? viewer.Track;
        var rows = await db.PerformanceSnapshots.AsNoTracking()
            .Where(s => s.UserId == viewer.UserId
                && s.Track == track
                && (s.Dimension == PerformanceDimension.Overall || s.Dimension == PerformanceDimension.ClientNeeds))
            .ToListAsync(cancellationToken);

        var overall = rows.SingleOrDefault(s => s.Dimension == PerformanceDimension.Overall);
        var names = await dimensionNames.ForTrackAsync(track, cancellationToken);
        return new PerformanceSummary(
            track,
            overall?.AttemptsCount ?? 0,
            overall?.ItemsAnswered ?? 0,
            overall?.ItemsCorrect ?? 0,
            overall?.Accuracy ?? 0m,
            overall?.AvgTimeMs ?? 0,
            [.. WeakestCategories.From(rows).Select(s => BreakdownRow.From(s, names))]);
    }
}
