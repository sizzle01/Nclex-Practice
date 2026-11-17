using System.Text.Json;
using Nclex.Domain.Timing;

namespace Nclex.Domain.Entities;

/// <summary>
/// One candidate's run through a test set: a draft while in progress or paused, then submitted or
/// abandoned (architecture.md 5.3). Maps to <c>attempts</c> (database.md section 2.6). Its items are
/// <see cref="AttemptItem"/> rows. Callers hold the row lock (<c>SELECT … FOR UPDATE</c>) before
/// calling any method that changes it.
/// </summary>
public sealed class Attempt
{
    /// <summary>A draft untouched for longer than this gets a "resume or discard?" prompt (architecture.md 5.3).</summary>
    public static readonly TimeSpan StaleDraftAge = TimeSpan.FromDays(30);

    // For EF Core, which sets properties directly when loading rows.
    private Attempt()
    {
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public Guid TestSetId { get; private set; }

    public ExamTrack Track { get; private set; }

    public AttemptStatus Status { get; private set; }

    /// <summary>The set's settings when the attempt started.</summary>
    public AttemptSettings Settings { get; private set; } = null!;

    public int TotalItems { get; private set; }

    /// <summary>The item the candidate is on, from 1 to <see cref="TotalItems"/>.</summary>
    public int CurrentPosition { get; private set; }

    /// <summary>Exam timer deadline while it runs (architecture.md 5.1).</summary>
    public DateTimeOffset? ExamDeadlineAt { get; private set; }

    /// <summary>Exam time left; authoritative while paused or before the timer first starts.</summary>
    public long? ExamRemainingMs { get; private set; }

    /// <summary>
    /// Bumped on pause and resume. Each delayed expiry job carries the epoch it was scheduled with,
    /// so a job from before a pause does nothing (architecture.md 5.1, rule 2).
    /// </summary>
    public int TimerEpoch { get; private set; }

    public short PausesUsed { get; private set; }

    public DateTimeOffset? PauseStartedAt { get; private set; }

    /// <summary>Why the attempt is paused; null while it is not.</summary>
    public PauseReason? PauseReason { get; private set; }

    public long TotalPausedMs { get; private set; }

    /// <summary>The exam timer; <see cref="Countdown.None"/> for an attempt without a time budget.</summary>
    public Countdown ExamTimer => new(ExamDeadlineAt, ExamRemainingMs);

    /// <summary>
    /// The device that may write to the attempt (architecture.md 5.2). Each page load of the player
    /// makes a fresh id and claims the lease with <see cref="Open"/>.
    /// </summary>
    public string? ActiveClientId { get; private set; }

    public DateTimeOffset? LeaseAcquiredAt { get; private set; }

    /// <summary>
    /// Adaptive-testing state (<see cref="Cat.CatState"/>, architecture.md 5.8); null for every
    /// other kind of attempt.
    /// </summary>
    public JsonDocument? CatState { get; private set; }

    /// <summary>True for an adaptive exam, whose items are added one at a time.</summary>
    public bool IsAdaptive => Settings.Mode == TestMode.Cat;

    public DateTimeOffset StartedAt { get; private set; }

    /// <summary>The last time the candidate did anything; drives the stale-draft prompt.</summary>
    public DateTimeOffset LastActivityAt { get; private set; }

    public DateTimeOffset? SubmittedAt { get; private set; }

    public SubmitReason? SubmitReason { get; private set; }

    /// <summary>In progress or paused: something the candidate can resume or discard.</summary>
    public bool IsDraft => Status is AttemptStatus.InProgress or AttemptStatus.Paused;

    /// <summary>The per-question limit in milliseconds, or null when items are not timed.</summary>
    private long? PerQuestionMs => Settings.PerQuestionSec * 1000L;

    /// <summary>
    /// Starts an attempt of <paramref name="set"/> serving <paramref name="questionVersionIds"/> in order.
    /// No device holds the lease yet, and the exam timer holds its full budget without running: it
    /// starts when the player calls <see cref="Open"/> (api.md, POST /attempts).
    /// </summary>
    public static (Attempt Attempt, IReadOnlyList<AttemptItem> Items) Start(
        Guid userId,
        TestSet set,
        IReadOnlyList<Guid> questionVersionIds,
        DateTimeOffset now)
    {
        if (questionVersionIds.Count == 0)
        {
            throw new ArgumentException("An attempt needs at least one item.", nameof(questionVersionIds));
        }

        var settings = AttemptSettings.From(set);
        var attempt = new Attempt
        {
            Id = Guid.CreateVersion7(now),
            UserId = userId,
            TestSetId = set.Id,
            Track = set.Track,
            Status = AttemptStatus.InProgress,
            Settings = settings,
            TotalItems = questionVersionIds.Count,
            CurrentPosition = 1,
            ExamRemainingMs = settings.TimeBudgetSec * 1000L,
            StartedAt = now,
            LastActivityAt = now,
        };
        return (attempt, AttemptItem.ForVersions(attempt.Id, questionVersionIds));
    }

    /// <summary>
    /// Starts an adaptive exam of <paramref name="set"/> with its first question (architecture.md 5.8).
    /// Later questions are added by <see cref="GiveNextAdaptiveItem"/>. With <paramref name="timed"/>
    /// false the attempt runs without timers, whatever the set says: the candidate chose (F19).
    /// </summary>
    public static (Attempt Attempt, AttemptItem FirstItem) StartAdaptive(
        Guid userId,
        TestSet set,
        bool timed,
        Guid firstQuestionVersionId,
        Cat.CatState catState,
        DateTimeOffset now)
    {
        if (set.Mode != TestMode.Cat)
        {
            throw new ArgumentException("Only an adaptive set starts an adaptive attempt.", nameof(set));
        }

        var settings = AttemptSettings.From(set);
        if (!timed)
        {
            settings = settings.WithoutTimers();
        }
        else if (settings.TimeBudgetSec is null)
        {
            throw new ArgumentException("A timed adaptive exam needs the set's time budget.", nameof(timed));
        }

        var attempt = new Attempt
        {
            Id = Guid.CreateVersion7(now),
            UserId = userId,
            TestSetId = set.Id,
            Track = set.Track,
            Status = AttemptStatus.InProgress,
            Settings = settings,
            TotalItems = 1,
            CurrentPosition = 1,
            ExamRemainingMs = settings.TimeBudgetSec * 1000L,
            CatState = catState.ToJson(),
            StartedAt = now,
            LastActivityAt = now,
        };
        return (attempt, AttemptItem.ForVersion(attempt.Id, 1, firstQuestionVersionId));
    }

    /// <summary>The adaptive state; throws for an attempt that is not adaptive.</summary>
    public Cat.CatState ReadCatState() => CatState is { } json
        ? Cat.CatState.FromJson(json)
        : throw new InvalidOperationException("This attempt is not adaptive.");

    /// <summary>
    /// Adaptive exam: the candidate confirmed the answer to the current item, which is final (there
    /// is no going back on the NCLEX). The item is locked so it can be graded straight away.
    /// </summary>
    public void LockAdaptiveAnswer(AttemptItem current, DateTimeOffset now)
    {
        EnsureAdaptiveInProgress();
        EnsureCurrent(current);
        current.Leave(now);
        current.StopTimer(now);
        current.Lock(now);
        LastActivityAt = now;
    }

    /// <summary>Stores the adaptive state after an answer, or when the exam stops.</summary>
    public void UpdateCatState(Cat.CatState state)
    {
        if (!IsAdaptive)
        {
            throw new InvalidOperationException("This attempt is not adaptive.");
        }

        CatState = state.ToJson();
    }

    /// <summary>
    /// Adaptive exam: adds <paramref name="questionVersionId"/> as the next item and moves to it.
    /// The item being left must already be locked by <see cref="LockAdaptiveAnswer"/>.
    /// </summary>
    public AttemptItem GiveNextAdaptiveItem(AttemptItem current, Guid questionVersionId, Cat.CatState state, DateTimeOffset now)
    {
        EnsureAdaptiveInProgress();
        EnsureCurrent(current);
        if (!current.IsLocked)
        {
            throw new InvalidOperationException("The current item must be answered before the next one is given.");
        }

        var next = AttemptItem.ForVersion(Id, TotalItems + 1, questionVersionId);
        TotalItems = next.Position;
        CurrentPosition = next.Position;
        next.Enter(now, PerQuestionMs);
        UpdateCatState(state);
        LastActivityAt = now;
        return next;
    }

    /// <summary>True for a draft nobody has touched for longer than <see cref="StaleDraftAge"/>.</summary>
    public bool IsStaleDraftAt(DateTimeOffset now) => IsDraft && now - LastActivityAt > StaleDraftAge;

    /// <summary>Whether <paramref name="clientId"/> may change the attempt now.</summary>
    public AttemptWriteCheck CheckWrite(string clientId) => Status switch
    {
        // The player routes to results on this code, whichever expiry path finalized the attempt.
        AttemptStatus.Submitted when SubmitReason == Entities.SubmitReason.TimeExpired => AttemptWriteCheck.TimeExpired,
        AttemptStatus.Submitted or AttemptStatus.Abandoned => AttemptWriteCheck.NotActive,
        AttemptStatus.Paused => AttemptWriteCheck.Paused,
        _ when ActiveClientId != clientId => AttemptWriteCheck.OpenedElsewhere,
        _ => AttemptWriteCheck.Allowed,
    };

    /// <summary>
    /// True when the exam timer ran out more than the write grace ago and the attempt is still in
    /// progress, so it must be finalized (architecture.md 5.1, rules 1 to 3). Paused attempts never
    /// expire: their timer is stopped.
    /// </summary>
    public bool HasExamExpiredAt(DateTimeOffset now) => Status == AttemptStatus.InProgress && ExamTimer.HasExpiredAt(now);

    /// <summary>
    /// Locks the current item with its answer when its timer has run out (PRD 5.2). Called on every
    /// write, before the write itself. True when it locked the item.
    /// </summary>
    public bool ExpireItemTimer(AttemptItem currentItem, DateTimeOffset now)
    {
        EnsureCurrent(currentItem);
        return Status == AttemptStatus.InProgress && currentItem.ExpireTimer(now);
    }

    /// <summary>
    /// The player loaded on a device: that device takes the lease from any other (architecture.md 5.2),
    /// and the time on the current item starts counting again. On the first open the exam timer
    /// starts; after that it keeps running from its deadline, even while no tab is open.
    /// </summary>
    public void Open(string clientId, AttemptItem currentItem, DateTimeOffset now)
    {
        EnsureDraft();
        EnsureCurrent(currentItem);

        ActiveClientId = clientId;
        LeaseAcquiredAt = now;

        // The previous device may have left the item open when its tab closed. Its time is counted
        // only up to its last write, so hours with the tab closed are not added to the item.
        // Only the time-on-item count is cut back: the item timer, like the exam timer, kept running.
        currentItem.Leave(LastActivityAt);
        if (Status == AttemptStatus.InProgress)
        {
            SetExamTimer(ExamTimer.StartAt(now));
            currentItem.Enter(now, PerQuestionMs);
        }

        LastActivityAt = now;
    }

    /// <summary>
    /// Pauses: a break or Save &amp; Exit, which follow the same rules. Both timers freeze, the pause
    /// counts against the limit, and the epoch moves on so the pending expiry job does nothing
    /// (architecture.md 5.1).
    /// </summary>
    public PauseOutcome Pause(PauseReason reason, AttemptItem currentItem, DateTimeOffset now)
    {
        EnsureCurrent(currentItem);
        if (Status != AttemptStatus.InProgress)
        {
            throw new InvalidOperationException($"Only an attempt in progress can be paused, not one that is {Status}.");
        }

        if (!AttemptTimer.CanPause(PausesUsed, Settings.PausePolicy.MaxPauses))
        {
            return PauseOutcome.LimitReached;
        }

        currentItem.Leave(now);
        currentItem.StopTimer(now);
        SetExamTimer(ExamTimer.StopAt(now));
        Status = AttemptStatus.Paused;
        PauseReason = reason;
        PauseStartedAt = now;
        PausesUsed++;
        TimerEpoch++;
        LastActivityAt = now;
        return PauseOutcome.Paused;
    }

    /// <summary>
    /// Resumes a paused attempt on <paramref name="clientId"/>, which takes the lease (architecture.md
    /// 5.3). Time past the pause limit comes off the exam time. When none is left the timers stay
    /// stopped and the result is <see cref="ResumeOutcome.TimeExpired"/>: the caller finalizes.
    /// </summary>
    public ResumeOutcome Resume(string clientId, AttemptItem currentItem, DateTimeOffset now)
    {
        EnsureCurrent(currentItem);
        if (Status != AttemptStatus.Paused || PauseStartedAt is not { } pauseStartedAt)
        {
            throw new InvalidOperationException($"Only a paused attempt can be resumed, not one that is {Status}.");
        }

        var exam = AttemptTimer.AfterPause(ExamTimer, pauseStartedAt, now, Settings.PausePolicy.MaxPauseSec);
        SetExamTimer(exam);
        TotalPausedMs += Math.Max(0, (long)(now - pauseStartedAt).TotalMilliseconds);
        Status = AttemptStatus.InProgress;
        PauseReason = null;
        PauseStartedAt = null;
        TimerEpoch++;
        ActiveClientId = clientId;
        LeaseAcquiredAt = now;
        LastActivityAt = now;

        if (exam.HasTimer && exam.RemainingMs <= 0)
        {
            return ResumeOutcome.TimeExpired;
        }

        SetExamTimer(exam.StartAt(now));
        currentItem.Enter(now, PerQuestionMs);
        return ResumeOutcome.Resumed;
    }

    /// <summary>
    /// Ends the attempt (status <c>submitted</c>) and locks every item with its current answer, so
    /// unanswered items count as blank (architecture.md 5.1). Grading happens next, in the same
    /// transaction (AttemptFinalizer).
    /// </summary>
    public void Finalize(SubmitReason reason, IReadOnlyList<AttemptItem> items, DateTimeOffset now)
    {
        EnsureDraft();
        if (items.Count != TotalItems || items.Any(i => i.AttemptId != Id))
        {
            throw new ArgumentException("Finalizing needs every item of this attempt.", nameof(items));
        }

        foreach (var item in items)
        {
            item.Leave(now);
            item.Lock(now);
        }

        if (PauseStartedAt is { } pauseStartedAt)
        {
            TotalPausedMs += Math.Max(0, (long)(now - pauseStartedAt).TotalMilliseconds);
        }

        SetExamTimer(ExamTimer.StopAt(now));
        Status = AttemptStatus.Submitted;
        SubmitReason = reason;
        SubmittedAt = now;
        PauseReason = null;
        PauseStartedAt = null;
        ActiveClientId = null;
        LastActivityAt = now;
    }

    /// <summary>
    /// Tutor mode: the candidate submitted <paramref name="item"/>, so its answer is final and it
    /// can be graded and revealed (architecture.md 5.5). Time after this is reading the rationale,
    /// so it is not counted as time on the item.
    /// </summary>
    public void SubmitItem(AttemptItem item, DateTimeOffset now)
    {
        if (Status != AttemptStatus.InProgress || Settings.RevealMode != RevealMode.Tutor)
        {
            throw new InvalidOperationException("Items are submitted one by one only in a tutor-mode attempt in progress.");
        }

        if (item.AttemptId != Id)
        {
            throw new InvalidOperationException("The item belongs to a different attempt.");
        }

        item.Leave(now);
        item.Lock(now);
        LastActivityAt = now;
    }

    /// <summary>
    /// Moves from the current item to <paramref name="to"/>. With back-navigation off, going back is
    /// refused and moving forward locks the item being left (api.md, POST /navigate).
    /// </summary>
    public NavigationOutcome Navigate(AttemptItem current, AttemptItem to, DateTimeOffset now)
    {
        EnsureCurrent(current);
        if (to.AttemptId != Id)
        {
            throw new InvalidOperationException("The target item belongs to a different attempt.");
        }

        if (!Settings.AllowBackNav && to.Position < CurrentPosition)
        {
            return NavigationOutcome.BackNavigationDisabled;
        }

        LastActivityAt = now;
        if (to.Position == CurrentPosition)
        {
            return NavigationOutcome.Moved;
        }

        current.Leave(now);
        current.StopTimer(now);
        if (!Settings.AllowBackNav)
        {
            current.Lock(now);
        }

        to.Enter(now, PerQuestionMs);
        CurrentPosition = to.Position;
        return NavigationOutcome.Moved;
    }

    /// <summary>Records that the candidate did something, such as saving an answer.</summary>
    public void RecordActivity(DateTimeOffset now) => LastActivityAt = now;

    /// <summary>Submitted attempts are kept for results and history, so only drafts can be discarded.</summary>
    public bool CanDiscard => Status != AttemptStatus.Submitted;

    /// <summary>Discards a draft (status <c>abandoned</c>). Discarding twice changes nothing.</summary>
    public void Discard(DateTimeOffset now)
    {
        if (!CanDiscard)
        {
            throw new InvalidOperationException("A submitted attempt cannot be discarded.");
        }

        if (Status == AttemptStatus.Abandoned)
        {
            return;
        }

        Status = AttemptStatus.Abandoned;
        ActiveClientId = null;
        LastActivityAt = now;
    }

    private void SetExamTimer(Countdown timer)
    {
        ExamDeadlineAt = timer.DeadlineAt;
        ExamRemainingMs = timer.RemainingMs;
    }

    private void EnsureDraft()
    {
        if (!IsDraft)
        {
            throw new InvalidOperationException($"The attempt is {Status}, not a draft.");
        }
    }

    private void EnsureAdaptiveInProgress()
    {
        if (!IsAdaptive || Status != AttemptStatus.InProgress)
        {
            throw new InvalidOperationException("Only an adaptive attempt in progress moves on one answer at a time.");
        }
    }

    private void EnsureCurrent(AttemptItem item)
    {
        if (item.AttemptId != Id || item.Position != CurrentPosition)
        {
            throw new InvalidOperationException("The item given is not this attempt's current item.");
        }
    }
}
