namespace Nclex.Domain.Entities;

/// <summary>
/// An uploaded image, audio clip or video that question payloads refer to by id.
/// Maps to <c>media_assets</c> (database.md section 2.4). Uploading arrives with admin authoring (F14).
/// </summary>
public sealed class MediaAsset
{
    // For EF Core, which sets properties directly when loading rows.
    private MediaAsset()
    {
    }

    public Guid Id { get; private set; }

    /// <summary>"image", "audio" or "video" (a CHECK constraint in the database).</summary>
    public string Kind { get; private set; } = string.Empty;

    /// <summary>The object's key in S3-compatible storage.</summary>
    public string StorageKey { get; private set; } = string.Empty;

    public string MimeType { get; private set; } = string.Empty;

    public long Bytes { get; private set; }

    public int? Width { get; private set; }

    public int? Height { get; private set; }

    public int? DurationMs { get; private set; }

    /// <summary>Required for images before a question using them is published (accessibility).</summary>
    public string? AltText { get; private set; }

    /// <summary>Storage key of the WebVTT captions; required for audio and video before publishing.</summary>
    public string? CaptionsKey { get; private set; }

    public Guid? CreatedBy { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
}
