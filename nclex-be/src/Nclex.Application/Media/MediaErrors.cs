using Nclex.Application.Common;

namespace Nclex.Application.Media;

/// <summary>Errors returned by the media endpoints (api.md section 1).</summary>
public static class MediaErrors
{
    public static readonly AppError NotFound = new(
        "NOT_FOUND",
        AppErrorKind.NotFound,
        "That media file was not found.");

    /// <summary>The link was changed or is past its expiry; the player fetches a fresh one.</summary>
    public static readonly AppError LinkInvalid = new(
        "FORBIDDEN",
        AppErrorKind.Forbidden,
        "This media link is invalid or has expired.");
}
