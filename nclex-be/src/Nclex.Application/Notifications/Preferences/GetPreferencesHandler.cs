using Nclex.Application.Abstractions;

namespace Nclex.Application.Notifications.Preferences;

/// <summary>GET /me/notification-preferences.</summary>
public sealed class GetPreferencesHandler(PreferenceRows preferences, ICurrentUser currentUser)
{
    public async Task<NotificationPreferencesResponse> HandleAsync(CancellationToken cancellationToken) =>
        PreferenceRows.ToResponse(await preferences.LoadAsync(currentUser.UserId, cancellationToken));
}
