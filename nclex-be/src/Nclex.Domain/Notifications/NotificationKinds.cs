namespace Nclex.Domain.Notifications;

/// <summary>
/// What a notification is about (database enum <c>notification_type</c>, database.md section 2.1).
/// Members are in the enum's order, which is also the order Postgres sorts them in.
/// </summary>
public enum NotificationType
{
    /// <summary>The score reached the set's pass threshold.</summary>
    ResultPass,

    /// <summary>The score fell short of the set's pass threshold.</summary>
    ResultFail,

    /// <summary>The set has no pass threshold, so there is only a score.</summary>
    ResultReady,

    /// <summary>A draft has had no activity for a while (architecture.md 5.3).</summary>
    DraftReminder,

    /// <summary>Phase 2.</summary>
    Streak,

    /// <summary>Phase 2.</summary>
    GoalMet,

    /// <summary>A content admin resolved the user's flag (F15).</summary>
    FlagResolved,

    /// <summary>Something changed on the account: password, email, lock, new sign-in.</summary>
    Security,

    /// <summary>Announcements from the platform.</summary>
    System,
}

/// <summary>
/// The group a notification belongs to; preferences are set per category (database enum
/// <c>notification_category</c>, architecture.md 5.6).
/// </summary>
public enum NotificationCategory
{
    Results,
    Reminders,
    Engagement,
    Content,
    Security,
}

/// <summary>Values of <c>notifications.channels</c>: where a notification was delivered.</summary>
public static class NotificationChannels
{
    /// <summary>Shown in the bell and on the notifications page.</summary>
    public const string InApp = "in_app";

    /// <summary>Sent as an email through the outbox.</summary>
    public const string Email = "email";
}
