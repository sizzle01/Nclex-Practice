namespace Nclex.Domain.Flags;

/// <summary>
/// A candidate's report of a problem with one question version, and its moderation. Maps to
/// <c>question_flags</c> (database.md section 2.9). A user reports a version at most once per
/// reason (the table's unique key).
/// </summary>
/// <remarks>
/// Status only moves forward: open, then optionally triaged, then resolved or rejected. A closed
/// report (resolved or rejected) never changes again, so the reporter is notified at most once.
/// </remarks>
public sealed class QuestionFlag
{
    /// <summary>Longest comment a reporter may write (the table's check constraint).</summary>
    public const int MaxCommentLength = 2000;

    // For EF Core, which sets properties directly when loading rows.
    private QuestionFlag()
    {
    }

    public Guid Id { get; private set; }

    /// <summary>Who reported it.</summary>
    public Guid UserId { get; private set; }

    public Guid QuestionVersionId { get; private set; }

    /// <summary>The attempt it was reported from, if any; cleared if that attempt is deleted.</summary>
    public Guid? AttemptId { get; private set; }

    public FlagReason Reason { get; private set; }

    public string? Comment { get; private set; }

    public FlagStatus Status { get; private set; }

    /// <summary>The reviewer or admin looking after it, if any.</summary>
    public Guid? AssigneeId { get; private set; }

    /// <summary>What the moderator decided. Sent to the reporter when the report is resolved.</summary>
    public string? ResolutionNote { get; private set; }

    /// <summary>Who resolved or rejected it.</summary>
    public Guid? ResolvedBy { get; private set; }

    public DateTimeOffset? ResolvedAt { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public bool IsClosed => Status is FlagStatus.Resolved or FlagStatus.Rejected;

    /// <summary>A new, open report. A blank comment is stored as none.</summary>
    public static QuestionFlag Report(
        Guid userId,
        Guid questionVersionId,
        Guid? attemptId,
        FlagReason reason,
        string? comment,
        DateTimeOffset now)
    {
        var trimmed = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
        if (trimmed?.Length > MaxCommentLength)
        {
            throw new ArgumentException($"A comment can be at most {MaxCommentLength} characters.", nameof(comment));
        }

        return new QuestionFlag
        {
            Id = Guid.CreateVersion7(now),
            UserId = userId,
            QuestionVersionId = questionVersionId,
            AttemptId = attemptId,
            Reason = reason,
            Comment = trimmed,
            Status = FlagStatus.Open,
            CreatedAt = now,
        };
    }

    /// <summary>
    /// Whether moderation may move a report from <paramref name="from"/> to <paramref name="to"/>.
    /// Staying where it is counts as allowed, so a moderator can change only the assignee.
    /// </summary>
    public static bool CanMove(FlagStatus from, FlagStatus to) => (from, to) switch
    {
        (FlagStatus.Resolved or FlagStatus.Rejected, _) => false,
        (_, FlagStatus.Open) => from == FlagStatus.Open,
        _ => true,
    };

    /// <summary>Hands the report to a moderator.</summary>
    public void AssignTo(Guid assigneeId)
    {
        EnsureOpenForChanges();
        AssigneeId = assigneeId;
    }

    /// <summary>Confirms the report needs work. Triaging a triaged report changes nothing.</summary>
    public void Triage() => MoveTo(FlagStatus.Triaged);

    /// <summary>Closes the report as fixed or answered; the caller notifies the reporter.</summary>
    public void Resolve(string note, Guid moderatorId, DateTimeOffset now) =>
        Close(FlagStatus.Resolved, note, moderatorId, now);

    /// <summary>Closes the report with no change needed.</summary>
    public void Reject(string note, Guid moderatorId, DateTimeOffset now) =>
        Close(FlagStatus.Rejected, note, moderatorId, now);

    private void Close(FlagStatus outcome, string note, Guid moderatorId, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(note))
        {
            throw new ArgumentException("Closing a report needs a note.", nameof(note));
        }

        MoveTo(outcome);
        ResolutionNote = note.Trim();
        ResolvedBy = moderatorId;
        ResolvedAt = now;
    }

    private void MoveTo(FlagStatus status)
    {
        if (!CanMove(Status, status))
        {
            throw new InvalidOperationException($"A {Status} report cannot move to {status}.");
        }

        Status = status;
    }

    private void EnsureOpenForChanges()
    {
        if (IsClosed)
        {
            throw new InvalidOperationException("A closed report cannot change.");
        }
    }
}
