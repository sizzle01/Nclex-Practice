using System.Globalization;
using Nclex.Application.Abstractions;

namespace Nclex.Application.Media;

/// <summary>The API paths that serve media files (api.md section 10).</summary>
public static class MediaPaths
{
    /// <summary>The path segment of each part: /media/{id}/file or /media/{id}/captions.</summary>
    public static string Segment(MediaPart part) => part == MediaPart.File ? "file" : "captions";

    /// <summary>
    /// A relative signed link. The browser resolves it against the app's own origin, and the
    /// Next.js server forwards /api to the API, so no host name is needed here.
    /// </summary>
    public static string SignedLink(Guid mediaId, MediaPart part, MediaLink link) =>
        string.Create(
            CultureInfo.InvariantCulture,
            $"/api/v1/media/{mediaId}/{Segment(part)}?exp={link.Expires}&sig={Uri.EscapeDataString(link.Signature)}");
}
