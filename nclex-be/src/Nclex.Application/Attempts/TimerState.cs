using Nclex.Domain.Entities;

namespace Nclex.Application.Attempts;

/// <summary>
/// The timer block every attempt response carries (api.md section 1, TimerState). The client
/// computes <c>offset = serverNow - Date.now()</c> so a wrong local clock cannot change the time.
/// </summary>
/// <param name="Exam">The exam timer; null for an untimed attempt.</param>
/// <param name="Item">The per-item timer; null when there is none.</param>
public sealed record TimerState(DateTimeOffset ServerNow, ClockState? Exam, ClockState? Item, PauseState Pause)
{
    /// <summary>
    /// F08 runs every attempt untimed, so both timers are null. F09 fills them in from the
    /// deadline columns (architecture.md 5.1).
    /// </summary>
    public static TimerState For(Attempt attempt, DateTimeOffset now) => new(
        now,
        Exam: null,
        Item: null,
        new PauseState(
            attempt.PausesUsed,
            attempt.Settings.PausePolicy.MaxPauses,
            attempt.Settings.PausePolicy.MaxPauseSec,
            attempt.PauseStartedAt));
}

/// <summary>One countdown: a deadline while running, the remaining time while stopped.</summary>
/// <param name="RemainingMs">Authoritative while stopped; for information while running.</param>
public sealed record ClockState(bool Running, DateTimeOffset? DeadlineAt, long RemainingMs);

/// <summary>Pauses used and allowed; a null limit means no limit.</summary>
public sealed record PauseState(int Used, int? Max, int? MaxPauseSec, DateTimeOffset? StartedAt);
