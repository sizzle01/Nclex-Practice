using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Application.Email;
using Nclex.Domain.Entities;
using Nclex.Domain.Notifications;
using Nclex.Domain.Timing;

namespace Nclex.Application.Notifications.DraftReminders;

/// <summary>
/// Run by the recurring DraftReminderJob: sends "Pick up where you left off" for every draft that
/// has been inactive for the configured time and has not been reminded since its last activity
/// (<see cref="DraftReminderPolicy"/>).
/// </summary>
public sealed class SendDraftRemindersHandler(
    IAppDbContext db,
    INotificationPublisher notifications,
    DraftReminderSettings settings,
    TimeProvider clock)
{
    /// <summary>Most reminders sent per run; the next run picks up the rest.</summary>
    public const int BatchSize = 200;

    /// <returns>How many drafts were reminded.</returns>
    public async Task<int> HandleAsync(CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        var due = (await DueDraftsAsync(now, cancellationToken))
            .Where(d => DraftReminderPolicy.IsDue(d.LastActivityAt, d.LastReminderAt, now, settings.Inactivity))
            .ToList();

        foreach (var draft in due)
        {
            await notifications.PublishAsync(Reminder(draft, now), cancellationToken);
        }

        await db.SaveChangesAsync(cancellationToken);
        return due.Count;
    }

    private async Task<List<DueDraft>> DueDraftsAsync(DateTimeOffset now, CancellationToken cancellationToken)
    {
        var inactiveSince = DraftReminderPolicy.InactiveSince(now, settings.Inactivity);

        // The "not reminded since" half of the policy is also applied here, so drafts that were
        // already reminded never fill the batch and starve newer ones.
        return await db.Attempts.AsNoTracking()
            .Where(a => (a.Status == AttemptStatus.InProgress || a.Status == AttemptStatus.Paused)
                && a.LastActivityAt <= inactiveSince
                && !db.Notifications.Any(n => n.RelatedAttemptId == a.Id
                    && n.Type == NotificationType.DraftReminder
                    && n.CreatedAt >= a.LastActivityAt)
                && db.Users.Any(u => u.Id == a.UserId && u.DeletedAt == null))
            .OrderBy(a => a.LastActivityAt)
            .Take(BatchSize)
            .Select(a => new DueDraft(
                a.Id,
                a.UserId,
                db.Users.Where(u => u.Id == a.UserId).Select(u => u.DisplayName).First(),
                db.TestSets.Where(s => s.Id == a.TestSetId).Select(s => s.Title).First(),
                a.TotalItems,
                db.AttemptItems.Count(i => i.AttemptId == a.Id
                    && (i.State == ItemState.Answered || (i.State == ItemState.Locked && i.Response != null))),
                a.ExamDeadlineAt,
                a.ExamRemainingMs,
                a.LastActivityAt,
                db.Notifications
                    .Where(n => n.RelatedAttemptId == a.Id && n.Type == NotificationType.DraftReminder)
                    .Max(n => (DateTimeOffset?)n.CreatedAt)))
            .ToListAsync(cancellationToken);
    }

    private static NewNotification Reminder(DueDraft draft, DateTimeOffset now)
    {
        var answeredShare = draft.TotalItems == 0 ? 0m : (decimal)draft.Answered / draft.TotalItems;
        var timeLeftMs = new Countdown(draft.ExamDeadlineAt, draft.ExamRemainingMs).RemainingAt(now);
        var deepLink = $"/attempts/{draft.AttemptId}";

        return new NewNotification(
            draft.UserId,
            NotificationType.DraftReminder,
            NotificationCategory.Reminders,
            NotificationCopy.DraftReminder(draft.SetTitle, answeredShare, timeLeftMs),
            new { pctComplete = Math.Round(answeredShare, 4), timeLeftMs, deepLink },
            draft.AttemptId,
            new NotificationEmail(
                EmailTemplateNames.DraftReminder,
                new DraftReminderEmailPayload(draft.DisplayName, draft.SetTitle, answeredShare, timeLeftMs, deepLink)));
    }

    private sealed record DueDraft(
        Guid AttemptId,
        Guid UserId,
        string? DisplayName,
        string SetTitle,
        int TotalItems,
        int Answered,
        DateTimeOffset? ExamDeadlineAt,
        long? ExamRemainingMs,
        DateTimeOffset LastActivityAt,
        DateTimeOffset? LastReminderAt);
}
