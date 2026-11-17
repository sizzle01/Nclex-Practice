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
/// <param name="Adaptive">The adaptive exam's progress (F19); null for every other attempt.</param>
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
    bool ResultAvailable,
    AdaptiveProgress? Adaptive);

/// <summary>
/// How far an adaptive exam has gone. It never shows the ability estimate while it runs, as on the
/// NCLEX: only the question count and the rules.
/// </summary>
/// <param name="Answered">Questions answered so far.</param>
/// <param name="MinItems">Questions before the exam may stop on a clear result.</param>
/// <param name="MaxItems">The exam always stops here.</param>
/// <param name="Timed">The candidate's choice when starting.</param>
public sealed record AdaptiveProgress(int Answered, int MinItems, int MaxItems, bool Timed)
{
    public static AdaptiveProgress? For(Attempt attempt)
    {
        if (!attempt.IsAdaptive)
        {
            return null;
        }

        var state = attempt.ReadCatState();
        return new AdaptiveProgress(state.AnsweredCount, state.Config.MinItems, state.Config.MaxItems, attempt.Settings.Timed);
    }
}

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
