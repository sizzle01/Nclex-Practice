namespace Nclex.Domain.Entities;

/// <summary>
/// An uploaded image, audio clip or video that question payloads refer to by id.
/// Maps to <c>media_assets</c> (database.md section 2.4). The content importer creates them (F11);
/// uploading arrives with admin authoring (F14).
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

    /// <summary>A new asset whose file is already in storage under <see cref="MediaDetails.StorageKey"/>.</summary>
    public static MediaAsset Create(Guid id, MediaDetails details, Guid? createdBy, DateTimeOffset now)
    {
        var asset = new MediaAsset { Id = id, CreatedBy = createdBy, CreatedAt = now };
        asset.Apply(details);
        return asset;
    }

    /// <summary>The asset as it is now, to compare with an incoming description.</summary>
    public MediaDetails Details => new(Kind, StorageKey, MimeType, Bytes, Width, Height, DurationMs, AltText, CaptionsKey);

    /// <summary>Replaces the description; returns false when nothing changed.</summary>
    public bool Update(MediaDetails details)
    {
        if (Details == details)
        {
            return false;
        }

        Apply(details);
        return true;
    }

    private void Apply(MediaDetails details)
    {
        Kind = details.Kind;
        StorageKey = details.StorageKey;
        MimeType = details.MimeType;
        Bytes = details.Bytes;
        Width = details.Width;
        Height = details.Height;
        DurationMs = details.DurationMs;
        AltText = details.AltText;
        CaptionsKey = details.CaptionsKey;
    }
}

/// <summary>Everything about a media asset except its identity and audit fields.</summary>
public sealed record MediaDetails(
    string Kind,
    string StorageKey,
    string MimeType,
    long Bytes,
    int? Width,
    int? Height,
    int? DurationMs,
    string? AltText,
    string? CaptionsKey);
