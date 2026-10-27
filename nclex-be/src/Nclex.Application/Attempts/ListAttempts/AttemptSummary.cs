using Nclex.Domain.Entities;

namespace Nclex.Application.Attempts.ListAttempts;

/// <summary>One row of the drafts list or the history table.</summary>
/// <param name="AnsweredItems">With <c>TotalItems</c>, gives the "% complete" on the dashboard.</param>
/// <param name="StaleDraft">True when the draft is over 30 days old, so the dashboard asks "resume or discard?".</param>
/// <param name="TimeRemainingMs">Exam time left; null for untimed attempts (every attempt until F09).</param>
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
    long? TimeRemainingMs);

/// <summary>Body of GET /attempts: one page, in the cursor format of api.md section 1.</summary>
/// <param name="NextCursor">Pass as <c>cursor</c> to get the next page; null on the last page.</param>
public sealed record AttemptPage(IReadOnlyList<AttemptSummary> Items, string? NextCursor);
