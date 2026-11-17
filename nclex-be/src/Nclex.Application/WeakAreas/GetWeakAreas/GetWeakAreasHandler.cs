using Nclex.Application.Catalog;
using Nclex.Domain.WeakAreas;

namespace Nclex.Application.WeakAreas.GetWeakAreas;

/// <summary>
/// GET /me/weak-areas: the user's weak areas and the size of the practice test they would get, so
/// the card can warn about a short test before the user starts it.
/// </summary>
public sealed class GetWeakAreasHandler(PlanTierGuard planTierGuard, WeakAreaFinder finder, WeakAreaPool pool)
{
    public async Task<WeakAreasView> HandleAsync(GetWeakAreasRequest request, CancellationToken cancellationToken)
    {
        var viewer = await planTierGuard.LoadViewerAsync(cancellationToken);
        var track = request.Track ?? viewer.Track;
        var found = await finder.FindAsync(viewer.UserId, track, cancellationToken);
        var loaded = await pool.LoadAsync(viewer, track, found.Areas, cancellationToken);
        var available = loaded.Candidates.Count;
        return new WeakAreasView(track, found.Areas, found.AttemptsConsidered, available, WeakAreaPractice.SizeFor(available));
    }
}
