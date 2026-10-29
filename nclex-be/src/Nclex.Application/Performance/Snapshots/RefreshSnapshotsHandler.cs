using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Domain.Entities;

namespace Nclex.Application.Performance.Snapshots;

/// <summary>
/// SnapshotRefreshJob (architecture.md 5.5): adds a newly graded attempt, and any other the
/// snapshots are missing, to the user's performance snapshots. Running it again changes nothing.
/// </summary>
public sealed class RefreshSnapshotsHandler(IAppDbContext db, SnapshotCatchUp catchUp)
{
    /// <summary>Returns how many attempts were added; 0 when the snapshots were already up to date.</summary>
    public async Task<int> HandleAsync(Guid userId, Guid attemptId, CancellationToken cancellationToken)
    {
        var track = await db.Attempts.AsNoTracking()
            .Where(a => a.Id == attemptId && a.UserId == userId && a.Status == AttemptStatus.Submitted)
            .Select(a => (ExamTrack?)a.Track)
            .SingleOrDefaultAsync(cancellationToken);
        if (track is null)
        {
            // The attempt was deleted with its user since the job was queued.
            return 0;
        }

        await using var transaction = await db.BeginTransactionAsync(cancellationToken);
        await db.LockPerformanceSnapshotsAsync(userId, cancellationToken);
        var added = await catchUp.RunAsync(userId, track.Value, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return added;
    }
}
