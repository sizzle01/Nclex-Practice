using Nclex.Domain.Entities;
using Nclex.Domain.Timing;

namespace Nclex.Application.Attempts;

/// <summary>
/// The timer block every attempt response carries (api.md section 1, TimerState). The client
/// computes <c>offset = serverNow - Date.now()</c> and shows <c>deadlineAt - (Date.now() + offset)</c>,
/// so a wrong local clock cannot change the time (architecture.md 5.1).
/// </summary>
/// <param name="Exam">The exam timer; null for an untimed attempt.</param>
/// <param name="Item">The current item's timer; null when the set has no per-question limit.</param>
public sealed record TimerState(DateTimeOffset ServerNow, ClockState? Exam, ClockState? Item, PauseState Pause)
{
    /// <summary>The timers of <paramref name="attempt"/>, with <paramref name="currentItemTimer"/> for the item it is on.</summary>
    public static TimerState For(Attempt attempt, Countdown currentItemTimer, DateTimeOffset now)
    {
        // An item not entered yet has no stored timer, but will start with the full limit.
        var itemTimer = currentItemTimer.HasTimer || attempt.Settings.PerQuestionSec is not { } perQuestionSec
            ? currentItemTimer
            : Countdown.Stopped(perQuestionSec * 1000L);
        return new TimerState(
            now,
            ClockState.From(attempt.ExamTimer, now),
            attempt.Settings.PerQuestionSec is null ? null : ClockState.From(itemTimer, now),
            new PauseState(
                attempt.PausesUsed,
                attempt.Settings.PausePolicy.MaxPauses,
                attempt.Settings.PausePolicy.MaxPauseSec,
                attempt.PauseStartedAt));
    }

    /// <inheritdoc cref="For(Attempt, Countdown, DateTimeOffset)"/>
    public static TimerState For(Attempt attempt, AttemptItem currentItem, DateTimeOffset now) =>
        For(attempt, currentItem.ItemTimer, now);
}

/// <summary>One countdown: a deadline while running, the remaining time while stopped.</summary>
/// <param name="RemainingMs">Authoritative while stopped; for information while running.</param>
public sealed record ClockState(bool Running, DateTimeOffset? DeadlineAt, long RemainingMs)
{
    /// <summary>Null when <paramref name="timer"/> is no timer at all.</summary>
    public static ClockState? From(Countdown timer, DateTimeOffset now) =>
        timer.RemainingAt(now) is { } remaining ? new ClockState(timer.IsRunning, timer.DeadlineAt, remaining) : null;
}

/// <summary>Pauses used and allowed; a null limit means no limit.</summary>
/// <param name="StartedAt">When the current pause began; null while not paused.</param>
public sealed record PauseState(int Used, int? Max, int? MaxPauseSec, DateTimeOffset? StartedAt);
