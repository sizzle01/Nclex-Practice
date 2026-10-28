using Nclex.Domain.Entities;

namespace Nclex.Application.Attempts.Grading;

/// <summary>
/// Grades attempt items and writes results (interfaces.md section 3, architecture.md 5.5). Every
/// method runs inside the caller's transaction, with the attempt row already locked FOR UPDATE.
/// </summary>
public interface IGradingService
{
    /// <summary>
    /// Tutor mode: grades the attempt's items that this request locked (submitted, timed out, or
    /// left with back-navigation off), so each one's rationale can be revealed straight away. Does
    /// nothing in exam mode, which grades everything at the end.
    /// </summary>
    Task GradeLockedTutorItemsAsync(Attempt attempt, CancellationToken cancellationToken);

    /// <summary>
    /// Grades every item not graded yet (a blank or timed-out item scores 0), then writes the result,
    /// its breakdown, the user's question history and the result notification.
    /// </summary>
    /// <param name="items">All the attempt's items, already locked by <see cref="Attempt.Finalize"/>.</param>
    /// <param name="seenPositions">Items the candidate looked at; only these count as used in the question history.</param>
    Task<AttemptResult> FinalizeAttemptAsync(
        Attempt attempt,
        IReadOnlyList<AttemptItem> items,
        IReadOnlySet<int> seenPositions,
        CancellationToken cancellationToken);
}
