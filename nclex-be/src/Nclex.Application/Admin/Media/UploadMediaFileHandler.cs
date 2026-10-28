using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Application.Common;
using Nclex.Application.Common.Validation;
using Nclex.Domain.Audit;

namespace Nclex.Application.Admin.Media;

/// <summary>
/// PUT /admin/media/{id}/upload: stores the file for a signed upload link. The signature is the
/// permission, as with a presigned S3 PUT, so it is checked before anything else. The file must
/// have the type and size declared when the link was made.
/// </summary>
public sealed class UploadMediaFileHandler(IAppDbContext db, IMediaStorage storage, IMediaUrlSigner signer, IAuditLog audit)
{
    public async Task HandleAsync(Guid mediaId, long expires, string? signature, string? contentType, Stream body, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(signature) || !signer.Verify(mediaId, MediaPart.Upload, expires, signature))
        {
            throw new AppException(AdminErrors.UploadLinkInvalid);
        }

        var asset = await db.MediaAssets.AsNoTracking().SingleOrDefaultAsync(m => m.Id == mediaId, cancellationToken)
            ?? throw new AppException(AdminErrors.MediaNotFound);
        if (!string.Equals(contentType, asset.MimeType, StringComparison.OrdinalIgnoreCase))
        {
            throw AppException.ForField("contentType", ValidationErrorCodes.InvalidValue);
        }

        var content = await ReadUpToAsync(body, asset.Bytes, cancellationToken);
        if (content is null || content.Length != asset.Bytes)
        {
            throw AppException.ForField("body", ValidationErrorCodes.InvalidValue);
        }

        await storage.SaveAsync(asset.StorageKey, content, cancellationToken);
        audit.RecordAdminEvent(AuditActions.MediaUploaded, asset.CreatedBy, AuditActions.MediaEntity, asset.Id, new { asset.Bytes });
        await db.SaveChangesAsync(cancellationToken);
    }

    // Reads at most one byte more than expected, so an oversized body is refused without buffering it all.
    private static async Task<byte[]?> ReadUpToAsync(Stream body, long expected, CancellationToken cancellationToken)
    {
        using var buffer = new MemoryStream();
        var chunk = new byte[81920];
        int read;
        while ((read = await body.ReadAsync(chunk, cancellationToken)) > 0)
        {
            buffer.Write(chunk, 0, read);
            if (buffer.Length > expected)
            {
                return null;
            }
        }

        return buffer.ToArray();
    }
}
