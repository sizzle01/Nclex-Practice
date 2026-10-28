using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Application.Common;

namespace Nclex.Application.Performance.RebuildSnapshots;

/// <summary>
/// POST /admin/performance-snapshots/rebuild (admin only): queues a rebuild of one user's
/// snapshots. The worker does the work (see Snapshots.RebuildSnapshotsHandler), so the request
/// returns at once.
/// </summary>
public sealed class RequestSnapshotRebuildHandler(IAppDbContext db, IJobScheduler jobs)
{
    public async Task HandleAsync(RebuildSnapshotsRequest request, CancellationToken cancellationToken)
    {
        // The validator has already refused a missing user id.
        var userId = request.UserId!.Value;
        if (!await db.Users.AnyAsync(u => u.Id == userId, cancellationToken))
        {
            throw new AppException(PerformanceErrors.UserNotFound);
        }

        jobs.EnqueueSnapshotRebuild(userId);
    }
}
