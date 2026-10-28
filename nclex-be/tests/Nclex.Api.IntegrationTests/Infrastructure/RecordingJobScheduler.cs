using System.Collections.Concurrent;
using Nclex.Application.Abstractions;

namespace Nclex.Api.IntegrationTests.Infrastructure;

/// <summary>
/// Stands in for Hangfire in timer tests: records each expiry job so a test can check what was
/// scheduled and then run the job itself at the moment it chooses.
/// </summary>
public sealed class RecordingJobScheduler : IJobScheduler
{
    private readonly ConcurrentQueue<ScheduledExpiry> _expiries = new();

    public IReadOnlyList<ScheduledExpiry> Expiries => [.. _expiries];

    public void ScheduleAttemptExpiry(Guid attemptId, int timerEpoch, DateTimeOffset at) =>
        _expiries.Enqueue(new ScheduledExpiry(attemptId, timerEpoch, at));

    public sealed record ScheduledExpiry(Guid AttemptId, int TimerEpoch, DateTimeOffset At);
}
