namespace Nclex.Domain.Entities;

/// <summary>
/// What a user has done with one question so far. Maps to <c>user_question_history</c>
/// (database.md section 2.7) and powers the builder's "unused" and "incorrect" pools.
/// Grading fills it in F10; until then it is empty.
/// </summary>
public sealed class UserQuestionHistory
{
    // For EF Core, which sets properties directly when loading rows.
    private UserQuestionHistory()
    {
    }

    public Guid UserId { get; private set; }

    public Guid QuestionId { get; private set; }

    /// <summary>How many graded attempts served this question to the user.</summary>
    public int TimesSeen { get; private set; }

    /// <summary>Whether the latest answer earned full points; null when it was never answered.</summary>
    public bool? LastCorrect { get; private set; }

    public DateTimeOffset LastSeenAt { get; private set; }

    /// <summary>The row for a question the user sees for the first time. F10's grading creates these.</summary>
    public static UserQuestionHistory FirstSeen(Guid userId, Guid questionId, bool? correct, DateTimeOffset seenAt) =>
        new()
        {
            UserId = userId,
            QuestionId = questionId,
            TimesSeen = 1,
            LastCorrect = correct,
            LastSeenAt = seenAt,
        };
}
