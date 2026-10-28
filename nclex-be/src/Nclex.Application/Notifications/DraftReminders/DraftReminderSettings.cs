using Nclex.Domain.Notifications;

namespace Nclex.Application.Notifications.DraftReminders;

/// <summary>
/// Configuration of the reminder job (section <c>Notifications</c> of the worker's settings).
/// </summary>
/// <param name="Inactivity">How long a draft must sit untouched before a reminder (architecture.md 5.3, default 3 days).</param>
public sealed record DraftReminderSettings(TimeSpan Inactivity)
{
    public static DraftReminderSettings Default { get; } = new(DraftReminderPolicy.DefaultInactivity);
}
