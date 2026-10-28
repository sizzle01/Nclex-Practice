using Nclex.Application.Common;

namespace Nclex.Application.Notifications;

/// <summary>Errors returned by the notification endpoints (api.md section 1).</summary>
public static class NotificationErrors
{
    /// <summary>The notification does not exist or belongs to someone else; both look the same to the caller.</summary>
    public static readonly AppError NotFound = new(
        "NOT_FOUND",
        AppErrorKind.NotFound,
        "That notification was not found.");

    /// <summary>
    /// Redis is down, so no stream can be opened. Not in api.md: the client treats it like a
    /// dropped stream and polls the unread count instead (architecture.md 5.6).
    /// </summary>
    public static readonly AppError StreamUnavailable = new(
        "STREAM_UNAVAILABLE",
        AppErrorKind.Unavailable,
        "Live notifications are unavailable right now.");

    /// <summary>The stream ticket is missing, used or expired.</summary>
    public static readonly AppError InvalidTicket = new(
        "UNAUTHENTICATED",
        AppErrorKind.Unauthenticated,
        "The stream ticket is invalid or has expired.");
}
