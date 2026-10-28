using Nclex.Application.Abstractions;

namespace Nclex.Application.Notifications.Preferences;

/// <summary>PUT /me/notification-preferences: replaces every category's channels.</summary>
public sealed class UpdatePreferencesHandler(PreferenceRows preferences, IAppDbContext db, ICurrentUser currentUser)
{
    public async Task<NotificationPreferencesResponse> HandleAsync(UpdatePreferencesRequest request, CancellationToken cancellationToken)
    {
        var rows = await preferences.LoadAsync(currentUser.UserId, cancellationToken);

        // The validator guarantees every category exactly once, with both channels set.
        foreach (var input in request.Preferences!)
        {
            rows.Single(r => r.Category == input.Category).Change(input.InApp!.Value, input.Email!.Value);
        }

        await db.SaveChangesAsync(cancellationToken);
        return PreferenceRows.ToResponse(rows);
    }
}
