using Nclex.Domain.Notifications;

namespace Nclex.Application.Notifications.Preferences;

/// <summary>Body of PUT /me/notification-preferences: every category, each exactly once.</summary>
public sealed record UpdatePreferencesRequest(IReadOnlyList<PreferenceInput>? Preferences);

/// <summary>One category's wanted channels. <c>locked</c>, if sent, is ignored.</summary>
public sealed record PreferenceInput(NotificationCategory? Category, bool? InApp, bool? Email);
