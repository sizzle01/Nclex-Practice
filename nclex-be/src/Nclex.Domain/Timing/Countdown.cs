namespace Nclex.Domain.Timing;

/// <summary>
/// One server-side countdown (the exam timer or an item timer), stored the way architecture.md 5.1
/// describes: a deadline while it runs, the remaining milliseconds while it is stopped. The server
/// never counts down; it only compares the clock with the deadline. A countdown with neither value
/// is "no timer". Pure value: every method returns a new countdown.
/// </summary>
/// <param name="DeadlineAt">When time runs out; set only while running.</param>
/// <param name="RemainingMs">Time left. Authoritative while stopped; the value at the last start while running.</param>
public readonly record struct Countdown(DateTimeOffset? DeadlineAt, long? RemainingMs)
{
    /// <summary>No timer at all (untimed exam, or no per-question limit).</summary>
    public static Countdown None => new(null, null);

    /// <summary>A stopped countdown with <paramref name="remainingMs"/> left.</summary>
    public static Countdown Stopped(long remainingMs) => new(null, Math.Max(0, remainingMs));

    public bool HasTimer => DeadlineAt is not null || RemainingMs is not null;

    public bool IsRunning => DeadlineAt is not null;

    /// <summary>Time left at <paramref name="now"/>, never below zero; null when there is no timer.</summary>
    public long? RemainingAt(DateTimeOffset now) => DeadlineAt is { } deadline
        ? Math.Max(0, (long)(deadline - now).TotalMilliseconds)
        : RemainingMs;

    /// <summary>Starts the countdown from its remaining time. Starting a running one changes nothing.</summary>
    public Countdown StartAt(DateTimeOffset now) => IsRunning || RemainingMs is not { } remaining
        ? this
        : new Countdown(now.AddMilliseconds(remaining), remaining);

    /// <summary>Freezes the countdown with the time left at <paramref name="now"/>.</summary>
    public Countdown StopAt(DateTimeOffset now) => IsRunning ? Stopped(RemainingAt(now)!.Value) : this;

    /// <summary>
    /// True once the deadline plus the write grace has passed (architecture.md 5.1, rule 1). The grace
    /// lets an answer sent in the last moment still arrive.
    /// </summary>
    public bool HasExpiredAt(DateTimeOffset now) => DeadlineAt is { } deadline && now > deadline + AttemptTimer.WriteGrace;
}
