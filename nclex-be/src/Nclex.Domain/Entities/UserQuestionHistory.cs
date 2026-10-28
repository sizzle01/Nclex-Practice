namespace Nclex.Domain.Entities;

/// <summary>
/// What a user has done with one question so far. Maps to <c>user_question_history</c>
/// (database.md section 2.7) and powers the builder's "unused" and "incorrect" pools.
/// Written by grading when an attempt is submitted.
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

    /// <summary>
    /// Whether the latest attempt earned full points. A question seen but left blank counts as
    /// incorrect, so it appears in the "incorrect" pool.
    /// </summary>
    public bool? LastCorrect { get; private set; }

    public DateTimeOffset LastSeenAt { get; private set; }

    /// <summary>The row for a question the user sees for the first time.</summary>
    public static UserQuestionHistory FirstSeen(Guid userId, Guid questionId, bool? correct, DateTimeOffset seenAt) =>
        new()
        {
            UserId = userId,
            QuestionId = questionId,
            TimesSeen = 1,
            LastCorrect = correct,
            LastSeenAt = seenAt,
        };

    /// <summary>Another graded attempt served the question again.</summary>
    public void RecordSeen(bool? correct, DateTimeOffset seenAt)
    {
        TimesSeen++;
        LastCorrect = correct;
        LastSeenAt = seenAt;
    }
}
