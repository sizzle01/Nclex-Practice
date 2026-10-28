namespace Nclex.Application.Media.GetMedia;

/// <summary>
/// A media asset as the player shows it (api.md section 10): signed links to the file and its
/// captions, and the alt text for images (interfaces.md section 4, Media).
/// </summary>
/// <param name="Id">The asset id that item payloads use as mediaId.</param>
/// <param name="Kind">image, audio or video.</param>
/// <param name="Url">A signed link to the file; it expires after a few minutes.</param>
/// <param name="CaptionsUrl">A signed link to the WebVTT captions, for audio and video.</param>
/// <param name="AltText">The image's text alternative.</param>
/// <param name="Width">Width in pixels, for images and video.</param>
/// <param name="Height">Height in pixels, for images and video.</param>
/// <param name="DurationMs">Length of an audio or video clip.</param>
/// <param name="ExpiresAt">When the links stop working.</param>
public sealed record MediaView(
    Guid Id,
    string Kind,
    string Url,
    string? CaptionsUrl,
    string? AltText,
    int? Width,
    int? Height,
    int? DurationMs,
    DateTimeOffset ExpiresAt);
