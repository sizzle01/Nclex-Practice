namespace Nclex.Domain.Notifications;

/// <summary>
/// The channel rules of architecture.md 5.6: results and security go to both channels by default,
/// everything else in-app only, and security cannot be switched off.
/// </summary>
public static class NotificationDefaults
{
    public static bool EmailByDefault(NotificationCategory category) =>
        category is NotificationCategory.Results or NotificationCategory.Security;

    /// <summary>
    /// Security notices tell the owner about changes they may not have made, so both channels are
    /// forced on (api.md section 6 shows <c>locked: true</c>).
    /// </summary>
    public static bool IsLocked(NotificationCategory category) => category == NotificationCategory.Security;

    /// <summary>
    /// The channels to deliver on, given the user's preference (or null if they have no row) and
    /// whether the notification has an email at all. Empty means "do not create it".
    /// </summary>
    public static IReadOnlyList<string> ChannelsFor(
        NotificationCategory category,
        bool? inAppPreference,
        bool? emailPreference,
        bool hasEmail)
    {
        var locked = IsLocked(category);
        var inApp = locked || (inAppPreference ?? true);
        var email = hasEmail && (locked || (emailPreference ?? EmailByDefault(category)));

        List<string> channels = [];
        if (inApp)
        {
            channels.Add(NotificationChannels.InApp);
        }

        if (email)
        {
            channels.Add(NotificationChannels.Email);
        }

        return channels;
    }
}
