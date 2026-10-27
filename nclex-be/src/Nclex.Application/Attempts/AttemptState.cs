using Nclex.Domain.Entities;

namespace Nclex.Application.Attempts;

/// <summary>
/// An attempt as the player sees it (api.md section 4, AttemptState). It has no item content: the
/// player fetches items one at a time.
/// </summary>
/// <param name="CurrentPosition">The item the candidate is on, from 1.</param>
/// <param name="Navigator">One entry per item, for the navigator grid.</param>
/// <param name="Timer">Server time and timers; the client works out its clock offset from <c>serverNow</c>.</param>
/// <param name="StaleDraft">True when the draft is over 30 days old, so the UI asks "resume or discard?".</param>
/// <param name="ResultAvailable">True once the attempt has been graded (F10).</param>
public sealed record AttemptState(
    Guid Id,
    TestSetRef TestSet,
    AttemptStatus Status,
    AttemptStateSettings Settings,
    int TotalItems,
    int CurrentPosition,
    AttemptProgress Progress,
    IReadOnlyList<NavigatorEntry> Navigator,
    TimerState Timer,
    DateTimeOffset StartedAt,
    DateTimeOffset LastActivityAt,
    bool StaleDraft,
    bool ResultAvailable);

/// <summary>The set an attempt belongs to.</summary>
public sealed record TestSetRef(Guid Id, string Title);

/// <summary>The settings the attempt runs under, copied from its set when it started.</summary>
public sealed record AttemptStateSettings(
    TestMode Mode,
    RevealMode RevealMode,
    bool AllowBackNav,
    int? PerQuestionSec,
    int? TimeBudgetSec,
    decimal? PassThreshold);

/// <summary>Counts for the top bar and the submit confirmation.</summary>
/// <param name="Answered">Items with a saved answer.</param>
/// <param name="Locked">Items that can no longer change.</param>
/// <param name="Flagged">Items marked for review.</param>
public sealed record AttemptProgress(int Answered, int Locked, int Flagged);

/// <summary>One cell of the navigator grid.</summary>
public sealed record NavigatorEntry(int Position, ItemState State, bool Flagged);
