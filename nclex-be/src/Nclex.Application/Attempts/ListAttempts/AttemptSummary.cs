using Nclex.Domain.Entities;

namespace Nclex.Application.Attempts.ListAttempts;

/// <summary>One row of the drafts list or the history table.</summary>
/// <param name="AnsweredItems">With <c>TotalItems</c>, gives the "% complete" on the dashboard.</param>
/// <param name="StaleDraft">True when the draft is over 30 days old, so the dashboard asks "resume or discard?".</param>
/// <param name="TimeRemainingMs">Exam time left now; null for untimed attempts.</param>
/// <param name="ScorePct">Share of points earned, 0 to 1, for the history table; null until submitted.</param>
/// <param name="Passed">Whether the attempt passed (an adaptive exam: its pass or fail); null without a result.</param>
/// <param name="ReadinessLabel">Pass, close to pass or fail (an adaptive exam: pass or fail); null without a result.</param>
/// <param name="Timed">Whether the attempt runs a timer; an adaptive exam's is the candidate's choice (F19).</param>
public sealed record AttemptSummary(
    Guid Id,
    TestSetRef TestSet,
    AttemptStatus Status,
    TestMode Mode,
    int TotalItems,
    int AnsweredItems,
    DateTimeOffset StartedAt,
    DateTimeOffset LastActivityAt,
    DateTimeOffset? SubmittedAt,
    bool StaleDraft,
    long? TimeRemainingMs,
    decimal? ScorePct,
    bool? Passed,
    ReadinessLabel? ReadinessLabel,
    bool Timed);

/// <summary>Body of GET /attempts: one page, in the cursor format of api.md section 1.</summary>
/// <param name="NextCursor">Pass as <c>cursor</c> to get the next page; null on the last page.</param>
public sealed record AttemptPage(IReadOnlyList<AttemptSummary> Items, string? NextCursor);
