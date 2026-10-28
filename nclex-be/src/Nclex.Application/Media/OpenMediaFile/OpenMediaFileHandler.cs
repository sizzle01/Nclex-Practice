using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Application.Common;

namespace Nclex.Application.Media.OpenMediaFile;

/// <summary>
/// GET /media/{id}/file and /media/{id}/captions: streams a file to anyone holding a valid signed
/// link. The signature is checked before the database is touched.
/// </summary>
public sealed class OpenMediaFileHandler(IAppDbContext db, IMediaStorage storage, IMediaUrlSigner signer)
{
    /// <summary>WebVTT's registered MIME type, used for every captions file.</summary>
    public const string CaptionsMimeType = "text/vtt";

    public async Task<MediaContent> HandleAsync(Guid mediaId, MediaPart part, long expires, string? signature, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(signature) || !signer.Verify(mediaId, part, expires, signature))
        {
            throw new AppException(MediaErrors.LinkInvalid);
        }

        var asset = await db.MediaAssets.AsNoTracking().SingleOrDefaultAsync(m => m.Id == mediaId, cancellationToken)
            ?? throw new AppException(MediaErrors.NotFound);
        var key = part == MediaPart.File ? asset.StorageKey : asset.CaptionsKey;
        var stream = key is null ? null : await storage.OpenReadAsync(key, cancellationToken);
        return stream is null
            ? throw new AppException(MediaErrors.NotFound)
            : new MediaContent(stream, part == MediaPart.File ? asset.MimeType : CaptionsMimeType);
    }
}

/// <summary>An open media file and the Content-Type to send it with. The caller disposes the stream.</summary>
public sealed record MediaContent(Stream Stream, string ContentType);
