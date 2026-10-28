using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Application.Common;

namespace Nclex.Application.Catalog.GetTestSet;

/// <summary>
/// GET /test-sets/{id}: one set's settings for the detail panel. A premium set is returned with
/// <c>locked: true</c> so the panel can explain why; only starting it (F08) is refused. The items
/// themselves are never part of the response.
/// </summary>
public sealed class GetTestSetHandler(IAppDbContext db, PlanTierGuard planTierGuard, LastAttempts lastAttempts)
{
    public async Task<TestSetDetail> HandleAsync(Guid id, CancellationToken cancellationToken)
    {
        var viewer = await planTierGuard.LoadViewerAsync(cancellationToken);
        var set = await db.TestSets.AsNoTracking()
            .Where(TestSetVisibility.VisibleTo(viewer.UserId))
            .SingleOrDefaultAsync(s => s.Id == id, cancellationToken)
            ?? throw new AppException(CatalogErrors.TestSetNotFound);

        var last = await lastAttempts.ForSetsAsync(viewer.UserId, [set.Id], cancellationToken);
        return TestSetDetail.From(set, viewer, last.GetValueOrDefault(set.Id));
    }
}
