using System.Collections.Concurrent;
using Nclex.Application.Abstractions;

namespace Nclex.Api.IntegrationTests.Infrastructure;

/// <summary>
/// Stands in for Hangfire: records each job so a test can check what was queued and then run the
/// job itself at the moment it chooses.
/// </summary>
public sealed class RecordingJobScheduler : IJobScheduler
{
    private readonly ConcurrentQueue<ScheduledExpiry> _expiries = new();
    private readonly ConcurrentQueue<SnapshotRefresh> _snapshotRefreshes = new();
    private readonly ConcurrentQueue<Guid> _snapshotRebuilds = new();

    public IReadOnlyList<ScheduledExpiry> Expiries => [.. _expiries];

    public IReadOnlyList<SnapshotRefresh> SnapshotRefreshes => [.. _snapshotRefreshes];

    /// <summary>The user ids whose snapshot rebuild was queued.</summary>
    public IReadOnlyList<Guid> SnapshotRebuilds => [.. _snapshotRebuilds];

    public void ScheduleAttemptExpiry(Guid attemptId, int timerEpoch, DateTimeOffset at) =>
        _expiries.Enqueue(new ScheduledExpiry(attemptId, timerEpoch, at));

    public void EnqueueSnapshotRefresh(Guid userId, Guid attemptId) =>
        _snapshotRefreshes.Enqueue(new SnapshotRefresh(userId, attemptId));

    public void EnqueueSnapshotRebuild(Guid userId) => _snapshotRebuilds.Enqueue(userId);

    public sealed record ScheduledExpiry(Guid AttemptId, int TimerEpoch, DateTimeOffset At);

    public sealed record SnapshotRefresh(Guid UserId, Guid AttemptId);
}
