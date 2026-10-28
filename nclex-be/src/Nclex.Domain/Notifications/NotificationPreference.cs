namespace Nclex.Domain.Notifications;

/// <summary>
/// Whether one user wants one category of notification in-app and by email. Maps to
/// <c>notification_preferences</c> (database.md section 2.8); every user has one row per category.
/// </summary>
public sealed class NotificationPreference
{
    // For EF Core, which sets properties directly when loading rows.
    private NotificationPreference()
    {
    }

    public Guid UserId { get; private set; }

    public NotificationCategory Category { get; private set; }

    public bool InApp { get; private set; }

    public bool Email { get; private set; }

    /// <summary>True when the user cannot change it (security notices always go out).</summary>
    public bool IsLocked => NotificationDefaults.IsLocked(Category);

    /// <summary>The rows a new account starts with, one per category (database.md section 2.8).</summary>
    public static IReadOnlyList<NotificationPreference> DefaultsFor(Guid userId) =>
        [.. Enum.GetValues<NotificationCategory>().Select(category => new NotificationPreference
        {
            UserId = userId,
            Category = category,
            InApp = true,
            Email = NotificationDefaults.EmailByDefault(category),
        })];

    /// <summary>Applies the user's choice. A locked category keeps both channels on.</summary>
    public void Change(bool inApp, bool email)
    {
        if (IsLocked)
        {
            return;
        }

        InApp = inApp;
        Email = email;
    }
}
