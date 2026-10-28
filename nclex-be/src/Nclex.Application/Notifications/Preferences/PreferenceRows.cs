using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Domain.Notifications;

namespace Nclex.Application.Notifications.Preferences;

/// <summary>Loads a user's preference rows, one per category.</summary>
public sealed class PreferenceRows(IAppDbContext db)
{
    /// <summary>
    /// The user's rows, tracked, in category order. Rows created at signup or by the F12 backfill
    /// should all exist; any missing one is added with its default, so callers always get all five.
    /// </summary>
    public async Task<IReadOnlyList<NotificationPreference>> LoadAsync(Guid userId, CancellationToken cancellationToken)
    {
        var rows = await db.NotificationPreferences
            .Where(p => p.UserId == userId)
            .ToListAsync(cancellationToken);

        foreach (var missing in NotificationPreference.DefaultsFor(userId).Where(d => rows.All(r => r.Category != d.Category)))
        {
            db.NotificationPreferences.Add(missing);
            rows.Add(missing);
        }

        return [.. rows.OrderBy(r => r.Category)];
    }

    public static NotificationPreferencesResponse ToResponse(IEnumerable<NotificationPreference> rows) =>
        new([.. rows.Select(PreferenceView.From)]);
}
