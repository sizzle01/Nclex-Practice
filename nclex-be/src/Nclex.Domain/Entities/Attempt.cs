using System.Text.Json;

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

    /// <summary>Exam timer deadline while it runs (architecture.md 5.1, F09).</summary>
    public DateTimeOffset? ExamDeadlineAt { get; private set; }

    /// <summary>Exam time left; authoritative while paused (F09).</summary>
    public long? ExamRemainingMs { get; private set; }

    /// <summary>Bumped on pause and resume so stale expiry jobs do nothing (F09).</summary>
    public int TimerEpoch { get; private set; }

    public short PausesUsed { get; private set; }

    public DateTimeOffset? PauseStartedAt { get; private set; }

    /// <summary>"break" or "save_exit" while paused (F09).</summary>
    public string? PauseReason { get; private set; }

    public long TotalPausedMs { get; private set; }

    /// <summary>
    /// The device that may write to the attempt (architecture.md 5.2). Each page load of the player
    /// makes a fresh id and claims the lease with <see cref="Open"/>.
    /// </summary>
    public string? ActiveClientId { get; private set; }

    public DateTimeOffset? LeaseAcquiredAt { get; private set; }

    /// <summary>Adaptive-testing state (phase 2).</summary>
    public JsonDocument? CatState { get; private set; }

    public DateTimeOffset StartedAt { get; private set; }

    /// <summary>The last time the candidate did anything; drives the stale-draft prompt.</summary>
    public DateTimeOffset LastActivityAt { get; private set; }

    public DateTimeOffset? SubmittedAt { get; private set; }

    public SubmitReason? SubmitReason { get; private set; }

    /// <summary>In progress or paused: something the candidate can resume or discard.</summary>
    public bool IsDraft => Status is AttemptStatus.InProgress or AttemptStatus.Paused;

    /// <summary>
    /// Starts an attempt of <paramref name="set"/> serving <paramref name="questionVersionIds"/> in order.
    /// No device holds the lease yet: the player calls <see cref="Open"/> when it loads.
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

        var attempt = new Attempt
        {
            Id = Guid.CreateVersion7(now),
            UserId = userId,
            TestSetId = set.Id,
            Track = set.Track,
            Status = AttemptStatus.InProgress,
            Settings = AttemptSettings.From(set),
            TotalItems = questionVersionIds.Count,
            CurrentPosition = 1,
            StartedAt = now,
            LastActivityAt = now,
        };
        return (attempt, AttemptItem.ForVersions(attempt.Id, questionVersionIds));
    }

    /// <summary>True for a draft nobody has touched for longer than <see cref="StaleDraftAge"/>.</summary>
    public bool IsStaleDraftAt(DateTimeOffset now) => IsDraft && now - LastActivityAt > StaleDraftAge;

    /// <summary>Whether <paramref name="clientId"/> may change the attempt now.</summary>
    public AttemptWriteCheck CheckWrite(string clientId) => Status switch
    {
        AttemptStatus.Submitted or AttemptStatus.Abandoned => AttemptWriteCheck.NotActive,
        AttemptStatus.Paused => AttemptWriteCheck.Paused,
        _ when ActiveClientId != clientId => AttemptWriteCheck.OpenedElsewhere,
        _ => AttemptWriteCheck.Allowed,
    };

    /// <summary>
    /// The player loaded on a device: that device takes the lease from any other (architecture.md 5.2),
    /// and the time on the current item starts counting again.
    /// </summary>
    public void Open(string clientId, AttemptItem currentItem, DateTimeOffset now)
    {
        EnsureDraft();
        EnsureCurrent(currentItem);

        ActiveClientId = clientId;
        LeaseAcquiredAt = now;

        // The previous device may have left the item open when its tab closed. Its time is counted
        // only up to its last write, so hours with the tab closed are not added to the item.
        currentItem.Leave(LastActivityAt);
        if (Status == AttemptStatus.InProgress)
        {
            currentItem.Enter(now);
        }

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
        if (!Settings.AllowBackNav)
        {
            current.Lock(now);
        }

        to.Enter(now);
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

    private void EnsureDraft()
    {
        if (!IsDraft)
        {
            throw new InvalidOperationException($"The attempt is {Status}, not a draft.");
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
