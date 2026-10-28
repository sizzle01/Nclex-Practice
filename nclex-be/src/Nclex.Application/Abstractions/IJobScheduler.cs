namespace Nclex.Application.Abstractions;

/// <summary>
/// Schedules background jobs (interfaces.md section 3). It wraps Hangfire so the use cases do not
/// reference it; the jobs themselves run in Nclex.Worker.
/// </summary>
public interface IJobScheduler
{
    /// <summary>
    /// Runs the expiry check for an attempt at <paramref name="at"/> (architecture.md 5.1, rule 2).
    /// The job does nothing if the attempt's timer epoch has moved on from <paramref name="timerEpoch"/>.
    /// </summary>
    void ScheduleAttemptExpiry(Guid attemptId, int timerEpoch, DateTimeOffset at);
}
