namespace Nclex.Domain.Notifications;

/// <summary>
/// A message for one user, such as "Your results are ready". Maps to <c>notifications</c>
/// (database.md section 2.8). Only rows delivered in-app appear in the bell and the feed; a row
/// delivered only by email is kept so a retry can see it was already sent.
/// </summary>
public sealed class Notification
{
    /// <summary>Types of which there is at most one per attempt (unique index <c>notif_result_once</c>).</summary>
    public static readonly IReadOnlyList<NotificationType> ResultTypes =
        [NotificationType.ResultPass, NotificationType.ResultFail, NotificationType.ResultReady];

    // For EF Core, which sets properties directly when loading rows.
    private Notification()
    {
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public NotificationType Type { get; private set; }

    public NotificationCategory Category { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string Body { get; private set; } = string.Empty;

    /// <summary>JSON values for the client, e.g. <c>{ scorePct, weakest, deepLink }</c> (stored as jsonb).</summary>
    public string Data { get; private set; } = "{}";

    /// <summary>The attempt it is about, if any; cleared if that attempt is deleted.</summary>
    public Guid? RelatedAttemptId { get; private set; }

    /// <summary>Where it was delivered: <see cref="NotificationChannels"/> values (a text[] column).</summary>
    public string[] Channels { get; private set; } = [];

    public DateTimeOffset? ReadAt { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public bool IsInApp => Channels.Contains(NotificationChannels.InApp);

    public bool IsRead => ReadAt is not null;

    public static Notification Create(
        Guid userId,
        NotificationType type,
        NotificationCategory category,
        string title,
        string body,
        string data,
        Guid? relatedAttemptId,
        IReadOnlyList<string> channels,
        DateTimeOffset now)
    {
        if (channels.Count == 0)
        {
            throw new ArgumentException("A notification must be delivered on at least one channel.", nameof(channels));
        }

        return new Notification
        {
            Id = Guid.CreateVersion7(now),
            UserId = userId,
            Type = type,
            Category = category,
            Title = title,
            Body = body,
            Data = data,
            RelatedAttemptId = relatedAttemptId,
            Channels = [.. channels],
            CreatedAt = now,
        };
    }

    /// <summary>Marks it read. Reading it again keeps the first time.</summary>
    /// <returns>True if it was unread.</returns>
    public bool MarkRead(DateTimeOffset now)
    {
        if (IsRead)
        {
            return false;
        }

        ReadAt = now;
        return true;
    }
}
