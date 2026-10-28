namespace Nclex.Application.Notifications.Feed;

/// <summary>Query string of GET /notifications (api.md section 6).</summary>
/// <param name="Cursor">The <c>nextCursor</c> of the previous page.</param>
/// <param name="UnreadOnly">Only notifications not yet read.</param>
/// <param name="Limit">Page size, 1 to 50; 20 when left out.</param>
public sealed record ListNotificationsRequest(string? Cursor = null, bool UnreadOnly = false, int? Limit = null)
{
    public const int DefaultLimit = 20;
    public const int MaxLimit = 50;
}

/// <summary>One page of the feed, newest first.</summary>
public sealed record NotificationPage(IReadOnlyList<NotificationView> Items, string? NextCursor);
