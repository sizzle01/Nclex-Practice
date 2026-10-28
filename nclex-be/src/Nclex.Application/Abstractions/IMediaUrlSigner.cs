namespace Nclex.Application.Abstractions;

/// <summary>
/// Signs short-lived links to a media file. Images and audio are loaded by the browser's own
/// &lt;img&gt; and &lt;audio&gt; elements, which can't send the in-memory access token, so the link
/// itself carries the permission, like a presigned S3 URL.
/// </summary>
public interface IMediaUrlSigner
{
    /// <summary>A signature for the file (or its captions) that stops working after the link lifetime.</summary>
    MediaLink Sign(Guid mediaId, MediaPart part);

    /// <summary>True when the signature belongs to this file and has not expired.</summary>
    bool Verify(Guid mediaId, MediaPart part, long expires, string signature);
}

/// <summary>Which file of a media asset a link opens.</summary>
public enum MediaPart
{
    /// <summary>The image, audio or video itself.</summary>
    File,

    /// <summary>The WebVTT captions of an audio or video clip.</summary>
    Captions,
}

/// <summary>A signed link's expiry (Unix seconds) and signature.</summary>
public sealed record MediaLink(long Expires, string Signature);
