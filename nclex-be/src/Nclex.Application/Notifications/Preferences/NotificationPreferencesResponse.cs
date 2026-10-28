using Nclex.Domain.Notifications;

namespace Nclex.Application.Notifications.Preferences;

/// <summary>Body of GET and PUT /me/notification-preferences (api.md section 6).</summary>
public sealed record NotificationPreferencesResponse(IReadOnlyList<PreferenceView> Preferences);

/// <summary>One category's channels.</summary>
/// <param name="Locked">True when the user cannot change it (security).</param>
public sealed record PreferenceView(NotificationCategory Category, bool InApp, bool Email, bool Locked)
{
    public static PreferenceView From(NotificationPreference preference) =>
        new(preference.Category, preference.InApp, preference.Email, preference.IsLocked);
}
