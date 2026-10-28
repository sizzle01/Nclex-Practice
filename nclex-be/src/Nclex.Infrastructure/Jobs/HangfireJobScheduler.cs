using Hangfire;
using Nclex.Application.Abstractions;

namespace Nclex.Infrastructure.Jobs;

/// <summary>Schedules jobs in Hangfire's Postgres storage; Nclex.Worker runs them.</summary>
internal sealed class HangfireJobScheduler(IBackgroundJobClient jobs) : IJobScheduler
{
    public void ScheduleAttemptExpiry(Guid attemptId, int timerEpoch, DateTimeOffset at) =>
        jobs.Schedule<AttemptExpiryJob>(job => job.RunAsync(attemptId, timerEpoch, CancellationToken.None), at);

    public void EnqueueSnapshotRefresh(Guid userId, Guid attemptId) =>
        jobs.Enqueue<SnapshotRefreshJob>(job => job.RunAsync(userId, attemptId, CancellationToken.None));

    public void EnqueueSnapshotRebuild(Guid userId) =>
        jobs.Enqueue<SnapshotRebuildJob>(job => job.RunAsync(userId, CancellationToken.None));
}
