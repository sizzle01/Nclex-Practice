namespace Nclex.Domain.Content;

/// <summary>
/// What authors may upload as question media (api.md section 8, POST /admin/media/upload-url).
/// SVG is left out on purpose: it can carry script, and media files are served from the app's origin.
/// </summary>
public static class MediaUploadRules
{
    /// <summary>25 MB: enough for a short clip or a large photo, and under the API's 30 MB request limit.</summary>
    public const long MaxBytes = 25L * 1024 * 1024;

    /// <summary>Captions and transcripts are sent as text in PATCH /admin/media/{id}.</summary>
    public const int MaxTextLength = 200_000;

    private static readonly Dictionary<string, (string Kind, string Extension)> Allowed = new(StringComparer.Ordinal)
    {
        ["image/png"] = ("image", "png"),
        ["image/jpeg"] = ("image", "jpg"),
        ["image/webp"] = ("image", "webp"),
        ["image/gif"] = ("image", "gif"),
        ["audio/mpeg"] = ("audio", "mp3"),
        ["audio/wav"] = ("audio", "wav"),
        ["audio/mp4"] = ("audio", "m4a"),
        ["video/mp4"] = ("video", "mp4"),
        ["video/webm"] = ("video", "webm"),
    };

    /// <summary>The MIME types an author may upload, for error messages and the upload form.</summary>
    public static IReadOnlyCollection<string> AllowedMimeTypes => Allowed.Keys;

    /// <summary>image, audio or video for an allowed MIME type; false for anything else.</summary>
    public static bool TryGetKind(string mimeType, out string kind)
    {
        var found = Allowed.TryGetValue(mimeType, out var entry);
        kind = found ? entry.Kind : string.Empty;
        return found;
    }

    /// <summary>Where an uploaded file is stored, e.g. "media/0192….png". The id keeps keys unique.</summary>
    public static string StorageKey(Guid mediaId, string mimeType) => $"media/{mediaId:N}.{Allowed[mimeType].Extension}";

    /// <summary>Where an asset's captions are stored.</summary>
    public static string CaptionsKey(Guid mediaId) => $"media/{mediaId:N}.vtt";

    /// <summary>Where an asset's transcript is stored.</summary>
    public static string TranscriptKey(Guid mediaId) => $"media/{mediaId:N}.txt";

    /// <summary>WebVTT files must start with this line (W3C WebVTT, file structure).</summary>
    public static bool IsWebVtt(string text) => text.TrimStart('﻿').StartsWith("WEBVTT", StringComparison.Ordinal);
}
