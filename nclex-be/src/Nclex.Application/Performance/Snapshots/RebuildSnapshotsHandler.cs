using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Domain.Entities;

namespace Nclex.Application.Performance.Snapshots;

/// <summary>
/// Throws away a user's performance snapshots and builds them again from their results. The
/// snapshots are derived data (database.md 2.7: "denormalized"), so this is always safe: it is the
/// repair for snapshots that went wrong, for example after a fix to the grading maths.
/// </summary>
public sealed class RebuildSnapshotsHandler(IAppDbContext db, SnapshotCatchUp catchUp)
{
    /// <summary>Returns how many attempts the new snapshots hold.</summary>
    public async Task<int> HandleAsync(Guid userId, CancellationToken cancellationToken)
    {
        await using var transaction = await db.BeginTransactionAsync(cancellationToken);
        await db.LockPerformanceSnapshotsAsync(userId, cancellationToken);
        await db.PerformanceSnapshots.Where(s => s.UserId == userId).ExecuteDeleteAsync(cancellationToken);

        var tracks = await db.Attempts.AsNoTracking()
            .Where(a => a.UserId == userId && a.Status == AttemptStatus.Submitted)
            .Select(a => a.Track)
            .Distinct()
            .ToListAsync(cancellationToken);
        var added = 0;
        foreach (var track in tracks)
        {
            added += await catchUp.RunAsync(userId, track, cancellationToken);
        }

        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return added;
    }
}
