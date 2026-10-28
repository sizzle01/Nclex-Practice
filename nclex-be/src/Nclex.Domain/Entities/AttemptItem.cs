using System.Text.Json;
using Nclex.Domain.Timing;

namespace Nclex.Domain.Entities;

/// <summary>
/// One item of an attempt: the question version served at a position, the candidate's answer and
/// how long they spent on it. Maps to <c>attempt_items</c> (database.md section 2.6). A locked item
/// is never written again.
/// </summary>
public sealed class AttemptItem
{
    // For EF Core, which sets properties directly when loading rows.
    private AttemptItem()
    {
    }

    public Guid AttemptId { get; private set; }

    /// <summary>1, 2, 3... in the order the items are served.</summary>
    public int Position { get; private set; }

    /// <summary>Fixed when the attempt starts, so editing the question later never changes this attempt.</summary>
    public Guid QuestionVersionId { get; private set; }

    public ItemState State { get; private set; }

    /// <summary>The candidate's answer, in the item type's response shape; null when unanswered.</summary>
    public JsonDocument? Response { get; private set; }

    /// <summary>The <c>clientSeq</c> of the last autosave that was applied; older writes are ignored.</summary>
    public int ClientSeq { get; private set; }

    /// <summary>"Mark for review", the candidate's own reminder; not a report about the content.</summary>
    public bool Flagged { get; private set; }

    /// <summary>Option ids the candidate has struck through.</summary>
    public IReadOnlyList<string> StruckOptions { get; private set; } = [];

    /// <summary>When the candidate arrived at the item; null while they are elsewhere.</summary>
    public DateTimeOffset? EnteredAt { get; private set; }

    /// <summary>Time on the item, worked out by the server from navigation (architecture.md 5.2).</summary>
    public long TimeSpentMs { get; private set; }

    /// <summary>Per-item timer deadline while it runs (architecture.md 5.1).</summary>
    public DateTimeOffset? ItemDeadlineAt { get; private set; }

    /// <summary>Per-item time left; authoritative while the timer is stopped.</summary>
    public int? ItemRemainingMs { get; private set; }

    /// <summary>
    /// The per-question timer. It runs only while the candidate is on the item, and freezes when
    /// they leave it or pause. No timer until the item is first entered in a set with one.
    /// </summary>
    public Countdown ItemTimer => new(ItemDeadlineAt, ItemRemainingMs);

    /// <summary>Filled in by grading (F10).</summary>
    public decimal? PointsEarned { get; private set; }

    /// <summary>Filled in by grading (F10).</summary>
    public short? MaxPoints { get; private set; }

    /// <summary>Filled in by grading (F10): true when the item earned full marks.</summary>
    public bool? IsCorrect { get; private set; }

    /// <summary>Filled in by grading (F10): the per-target +1/0/-1 breakdown.</summary>
    public JsonDocument? GradeDetail { get; private set; }

    public DateTimeOffset? GradedAt { get; private set; }

    public DateTimeOffset? LockedAt { get; private set; }

    /// <summary>When the current answer was saved; null when unanswered.</summary>
    public DateTimeOffset? AnsweredAt { get; private set; }

    public bool IsLocked => State == ItemState.Locked;

    public bool IsAnswered => State == ItemState.Answered || (State == ItemState.Locked && Response is not null);

    /// <summary>Creates an attempt's items, numbering them from 1 in the order given.</summary>
    public static IReadOnlyList<AttemptItem> ForVersions(Guid attemptId, IEnumerable<Guid> questionVersionIds) =>
        [.. questionVersionIds.Select((versionId, index) => new AttemptItem
        {
            AttemptId = attemptId,
            Position = index + 1,
            QuestionVersionId = versionId,
            State = ItemState.Unseen,
        })];

    /// <summary>
    /// Autosave. A null response clears the answer. The caller has already checked the response
    /// against the item type, and holds the attempt's row lock.
    /// </summary>
    public SaveOutcome SaveResponse(JsonDocument? response, int clientSeq, DateTimeOffset now)
    {
        if (IsLocked)
        {
            return SaveOutcome.ItemLocked;
        }

        // A retry can arrive after a newer write; it must not undo the newer answer.
        if (clientSeq <= ClientSeq)
        {
            return SaveOutcome.Stale;
        }

        Response = response;
        ClientSeq = clientSeq;
        State = response is null ? ItemState.Seen : ItemState.Answered;
        AnsweredAt = response is null ? null : now;
        return SaveOutcome.Saved;
    }

    /// <summary>Changes the candidate's marks; null leaves a mark as it is. False when the item is locked.</summary>
    public bool UpdateMarks(bool? flagged, IReadOnlyList<string>? struckOptions)
    {
        if (IsLocked)
        {
            return false;
        }

        Flagged = flagged ?? Flagged;
        StruckOptions = struckOptions is null ? StruckOptions : [.. struckOptions.Distinct(StringComparer.Ordinal)];
        return true;
    }

    /// <summary>True when the item timer has run out (with the write grace) and the item is not locked yet.</summary>
    public bool HasTimerExpiredAt(DateTimeOffset now) => !IsLocked && ItemTimer.HasExpiredAt(now);

    /// <summary>
    /// The candidate arrived at this item: start counting its time and, in a set with a per-question
    /// limit, start its timer from where it stopped (<paramref name="perQuestionMs"/> on the first visit).
    /// </summary>
    internal void Enter(DateTimeOffset now, long? perQuestionMs)
    {
        // Looking back at a locked item is review, not answering time, and locked rows stay as they are.
        if (IsLocked)
        {
            return;
        }

        EnteredAt = now;
        if (State == ItemState.Unseen)
        {
            State = ItemState.Seen;
        }

        if (perQuestionMs is null)
        {
            return;
        }

        var timer = ItemTimer.HasTimer ? ItemTimer : Countdown.Stopped(perQuestionMs.Value);
        if (!timer.IsRunning && timer.RemainingMs <= 0)
        {
            // Its time was used up on an earlier visit: PRD 5.2 locks it with the answer it has.
            Lock(now);
            return;
        }

        SetTimer(timer.StartAt(now));
    }

    /// <summary>Freezes the item timer, for leaving the item or pausing.</summary>
    internal void StopTimer(DateTimeOffset now) => SetTimer(ItemTimer.StopAt(now));

    /// <summary>
    /// Locks the item with its current answer when its timer has run out (PRD 5.2). Its time on the
    /// item stops at the deadline. False when the timer has not run out.
    /// </summary>
    internal bool ExpireTimer(DateTimeOffset now)
    {
        if (!HasTimerExpiredAt(now))
        {
            return false;
        }

        Leave(ItemDeadlineAt!.Value);
        Lock(now);
        return true;
    }

    /// <summary>The candidate left this item (or is treated as having left at <paramref name="until"/>).</summary>
    internal void Leave(DateTimeOffset until)
    {
        if (EnteredAt is not { } enteredAt)
        {
            return;
        }

        TimeSpentMs += Math.Max(0, (long)(until - enteredAt).TotalMilliseconds);
        EnteredAt = null;
    }

    /// <summary>Makes the current answer final.</summary>
    internal void Lock(DateTimeOffset now)
    {
        if (IsLocked)
        {
            return;
        }

        StopTimer(now);
        State = ItemState.Locked;
        LockedAt = now;
        EnteredAt = null;
    }

    private void SetTimer(Countdown timer)
    {
        ItemDeadlineAt = timer.DeadlineAt;
        // Never more than the set's per-question limit, which is a few minutes, so it fits an int.
        ItemRemainingMs = timer.RemainingMs is { } remaining ? (int)remaining : null;
    }
}
