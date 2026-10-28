using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Application.Attempts.Results;

namespace Nclex.Application.Performance.GetBreakdown;

/// <summary>GET /me/performance/breakdown: the snapshot rows of one dimension, a single indexed read.</summary>
public sealed class GetBreakdownHandler(IAppDbContext db, PerformanceViewers viewers, DimensionNames dimensionNames)
{
    public async Task<PerformanceBreakdown> HandleAsync(GetBreakdownRequest request, CancellationToken cancellationToken)
    {
        // The validator has already refused a missing dimension.
        var dimension = request.Dimension!.Value;
        var viewer = await viewers.CurrentAsync(cancellationToken);
        var track = request.Track ?? viewer.Track;
        var rows = await db.PerformanceSnapshots.AsNoTracking()
            .Where(s => s.UserId == viewer.UserId && s.Track == track && s.Dimension == dimension)
            .ToListAsync(cancellationToken);

        var names = await dimensionNames.ForTrackAsync(track, cancellationToken);
        var items = rows
            .Select(s => BreakdownRow.From(s, names))
            .OrderBy(r => r.Name, StringComparer.Ordinal)
            .ToList();
        return new PerformanceBreakdown(track, dimension, items);
    }
}
