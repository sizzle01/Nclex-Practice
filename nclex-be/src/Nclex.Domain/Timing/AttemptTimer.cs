namespace Nclex.Domain.Timing;

/// <summary>
/// The timer rules of architecture.md 5.1 as pure functions: pause limits, the pause-overrun rule,
/// and when each of the three expiry checks fires. The clock is always passed in, so tests can
/// fake it; <see cref="Entities.Attempt"/> applies these rules to its stored columns.
/// </summary>
public static class AttemptTimer
{
    /// <summary>A write this soon after the deadline is still accepted, so a last-second save arrives (rule 1).</summary>
    public static readonly TimeSpan WriteGrace = TimeSpan.FromSeconds(2);

    /// <summary>The sweeper only finalizes attempts this far past their deadline (rule 3).</summary>
    public static readonly TimeSpan SweepDelay = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Whether another pause is allowed. A null limit means unlimited (practice mode). Breaks and
    /// Save &amp; Exit both count, so Save &amp; Exit cannot be used to get round the break limit.
    /// </summary>
    public static bool CanPause(int pausesUsed, int? maxPauses) => maxPauses is not { } max || pausesUsed < max;

    /// <summary>
    /// How much exam time a pause costs: the time it ran past <paramref name="maxPauseSec"/>, as at a
    /// real testing centre. Nothing when there is no limit or the pause stayed within it.
    /// </summary>
    public static long OverrunMs(DateTimeOffset pauseStartedAt, DateTimeOffset now, int? maxPauseSec)
    {
        if (maxPauseSec is not { } limitSec)
        {
            return 0;
        }

        var pausedMs = (long)(now - pauseStartedAt).TotalMilliseconds;
        return Math.Max(0, pausedMs - (limitSec * 1000L));
    }

    /// <summary>
    /// The exam countdown after a pause: <c>remaining = remaining_at_pause − max(0, paused − maxPauseSec)</c>.
    /// It stays stopped; the caller starts it. Zero means the overrun used up the exam time.
    /// </summary>
    public static Countdown AfterPause(Countdown paused, DateTimeOffset pauseStartedAt, DateTimeOffset now, int? maxPauseSec) =>
        paused.RemainingMs is { } remaining
            ? Countdown.Stopped(remaining - OverrunMs(pauseStartedAt, now, maxPauseSec))
            : paused;

    /// <summary>
    /// When the delayed expiry job should run for a deadline (rule 2): just after the write grace,
    /// so the job and the write check agree that the time is up.
    /// </summary>
    public static DateTimeOffset ExpiryJobAt(DateTimeOffset deadline) => deadline + WriteGrace + TimeSpan.FromSeconds(1);

    /// <summary>The sweeper finalizes running attempts whose deadline is before this (rule 3).</summary>
    public static DateTimeOffset SweepCutoff(DateTimeOffset now) => now - SweepDelay;
}
