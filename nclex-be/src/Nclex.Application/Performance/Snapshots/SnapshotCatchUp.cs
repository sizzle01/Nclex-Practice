using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Domain.Entities;
using Nclex.Domain.Performance;

namespace Nclex.Application.Performance.Snapshots;

/// <summary>
/// Brings one user's snapshots for one track up to date with their submitted attempts. Shared by
/// the refresh job and the admin rebuild. The caller holds the user's snapshot lock
/// (<see cref="IAppDbContext.LockPerformanceSnapshotsAsync"/>), saves and commits.
/// </summary>
/// <remarks>
/// Attempts are added in submit order, (submitted_at, id). The overall row's last_attempt_id is the
/// high-water mark: every attempt up to it is already in the totals, so only later ones are added.
/// That makes the job idempotent (a second run finds nothing to add) and safe when jobs run out of
/// order (the first job to run adds every attempt so far; the others find nothing left). If the
/// overall row's attempts_count does not match the attempts up to the mark, an attempt committed
/// late landed behind it, so the track is replayed from scratch instead.
/// </remarks>
public sealed class SnapshotCatchUp(IAppDbContext db, TimeProvider clock)
{
    /// <summary>Adds the attempts the snapshots are missing; returns how many were added.</summary>
    public async Task<int> RunAsync(Guid userId, ExamTrack track, CancellationToken cancellationToken)
    {
        var submitted = await SubmittedInOrderAsync(userId, track, cancellationToken);
        var overall = await db.PerformanceSnapshots.AsNoTracking()
            .SingleOrDefaultAsync(Snapshots.Overall(userId, track), cancellationToken);

        var applied = overall?.AttemptsCount ?? 0;
        var markIndex = overall?.LastAttemptId is { } mark ? submitted.IndexOf(mark) : -1;
        if (markIndex + 1 != applied)
        {
            await db.PerformanceSnapshots.Where(s => s.UserId == userId && s.Track == track).ExecuteDeleteAsync(cancellationToken);
            markIndex = -1;
        }

        var pending = submitted.Skip(markIndex + 1).ToList();
        if (pending.Count > 0)
        {
            await AddAsync(userId, track, pending, cancellationToken);
        }

        return pending.Count;
    }

    /// <summary>Ids of the user's submitted attempts in the track, oldest first.</summary>
    private async Task<List<Guid>> SubmittedInOrderAsync(Guid userId, ExamTrack track, CancellationToken cancellationToken) =>
        await (
                from attempt in db.Attempts
                join result in db.AttemptResults on attempt.Id equals result.AttemptId
                where attempt.UserId == userId && attempt.Track == track && attempt.Status == AttemptStatus.Submitted
                orderby attempt.SubmittedAt, attempt.Id
                select attempt.Id)
            .ToListAsync(cancellationToken);

    private async Task AddAsync(Guid userId, ExamTrack track, List<Guid> attemptIds, CancellationToken cancellationToken)
    {
        var results = await db.AttemptResults.AsNoTracking()
            .Where(r => attemptIds.Contains(r.AttemptId))
            .ToDictionaryAsync(r => r.AttemptId, cancellationToken);
        var scores = (await db.AttemptCategoryScores.AsNoTracking()
                .Where(s => attemptIds.Contains(s.AttemptId))
                .ToListAsync(cancellationToken))
            .ToLookup(s => s.AttemptId);
        var rows = await db.PerformanceSnapshots
            .Where(s => s.UserId == userId && s.Track == track)
            .ToDictionaryAsync(s => (s.Dimension, s.DimensionKey), cancellationToken);

        var now = clock.GetUtcNow();
        PerformanceSnapshot Row(PerformanceDimension dimension, string key)
        {
            if (!rows.TryGetValue((dimension, key), out var row))
            {
                row = PerformanceSnapshot.Start(userId, track, dimension, key, now);
                rows.Add((dimension, key), row);
                db.PerformanceSnapshots.Add(row);
            }

            return row;
        }

        foreach (var attemptId in attemptIds)
        {
            Row(PerformanceDimension.Overall, PerformanceDimensions.OverallKey).Add(attemptId, SnapshotTally.From(results[attemptId]), now);
            foreach (var score in scores[attemptId])
            {
                Row(PerformanceDimensions.From(score.Dimension), score.DimensionKey).Add(attemptId, SnapshotTally.From(score), now);
            }
        }
    }
}
