namespace Nclex.Domain.Notifications;

/// <summary>
/// When a draft earns a "Pick up where you left off" reminder (architecture.md 5.3): after
/// <c>N</c> days without activity, once per stretch of inactivity. Any activity after a reminder
/// starts a new stretch.
/// </summary>
public static class DraftReminderPolicy
{
    /// <summary>The default for <c>N</c>; configurable per deployment.</summary>
    public static readonly TimeSpan DefaultInactivity = TimeSpan.FromDays(3);

    /// <summary>Drafts whose last activity is at or before this time are old enough for a reminder.</summary>
    public static DateTimeOffset InactiveSince(DateTimeOffset now, TimeSpan inactivity) => now - inactivity;

    /// <param name="lastActivityAt">The draft's <c>last_activity_at</c>.</param>
    /// <param name="lastReminderAt">When the newest reminder for this draft was created, if any.</param>
    public static bool IsDue(DateTimeOffset lastActivityAt, DateTimeOffset? lastReminderAt, DateTimeOffset now, TimeSpan inactivity) =>
        lastActivityAt <= InactiveSince(now, inactivity)
        && (lastReminderAt is not { } reminded || reminded < lastActivityAt);
}
