using Nclex.Domain.ItemTypes;

namespace Nclex.Domain.Content;

/// <summary>
/// Rules for a media asset before questions may use it (database.md 2.4, media_assets): images
/// need alt text and audio or video need WebVTT captions and a transcript, so every candidate can follow them
/// (interfaces.md section 4, Media).
/// </summary>
public static class MediaRules
{
    /// <summary>The kinds allowed by the media_assets CHECK constraint.</summary>
    public static readonly IReadOnlySet<string> Kinds = ExhibitRules.MediaKinds;

    /// <summary>The problems with one asset's description. Paths start at <paramref name="path"/>.</summary>
    /// <param name="path">Where the asset is, e.g. "media[0]".</param>
    /// <param name="kind">image, audio or video.</param>
    /// <param name="mimeType">Must match the kind, e.g. "image/png" for an image.</param>
    /// <param name="altText">The image's text alternative.</param>
    /// <param name="hasCaptions">Whether captions come with the asset.</param>
    /// <param name="hasTranscript">Whether a transcript comes with the asset.</param>
    public static IEnumerable<ValidationIssue> Check(string path, string kind, string mimeType, string? altText, bool hasCaptions, bool hasTranscript)
    {
        if (!Kinds.Contains(kind))
        {
            yield return new ValidationIssue($"{path}.kind", ValidationIssueCodes.InvalidValue, "The kind must be image, audio or video.");
            yield break;
        }

        if (!mimeType.StartsWith($"{kind}/", StringComparison.Ordinal))
        {
            yield return new ValidationIssue($"{path}.mimeType", ValidationIssueCodes.Mismatch, $"An {kind} needs a {kind}/… MIME type.");
        }

        if (kind == "image" && string.IsNullOrWhiteSpace(altText))
        {
            yield return new ValidationIssue($"{path}.altText", ValidationIssueCodes.Required, "An image needs alt text.");
        }

        if (kind != "image" && !hasCaptions)
        {
            yield return new ValidationIssue($"{path}.captionsFile", ValidationIssueCodes.Required, $"An {kind} clip needs a WebVTT captions file.");
        }

        // A transcript is the text alternative for audio (WCAG 1.2.1); captions need the clip to play.
        if (kind != "image" && !hasTranscript)
        {
            yield return new ValidationIssue($"{path}.transcriptFile", ValidationIssueCodes.Required, $"An {kind} clip needs a plain-text transcript file.");
        }
    }
}
