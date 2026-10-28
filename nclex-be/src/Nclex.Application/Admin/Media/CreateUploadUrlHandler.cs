using Nclex.Application.Abstractions;
using Nclex.Application.Media;
using Nclex.Domain.Audit;
using Nclex.Domain.Content;
using Nclex.Domain.Entities;

namespace Nclex.Application.Admin.Media;

/// <summary>
/// POST /admin/media/upload-url: records the asset and returns a signed link to upload its file.
/// The link points at this API, which stores the file through <see cref="IMediaStorage"/>; an S3 or
/// MinIO storage can later return a real presigned URL here instead (F14 walkthrough, section 6).
/// </summary>
public sealed class CreateUploadUrlHandler(
    IAppDbContext db,
    IMediaUrlSigner signer,
    ICurrentUser currentUser,
    IAuditLog audit,
    TimeProvider clock)
{
    public async Task<UploadUrlResponse> HandleAsync(CreateUploadUrlRequest request, CancellationToken cancellationToken)
    {
        MediaUploadRules.TryGetKind(request.MimeType, out var kind);
        var now = clock.GetUtcNow();
        var id = Guid.CreateVersion7(now);
        var details = new MediaDetails(kind, MediaUploadRules.StorageKey(id, request.MimeType), request.MimeType, request.Bytes, null, null, null, null, null, null);
        db.MediaAssets.Add(MediaAsset.Create(id, details, currentUser.UserId, now));
        audit.RecordAdminEvent(AuditActions.MediaCreated, currentUser.UserId, AuditActions.MediaEntity, id, new { request.MimeType, request.Bytes });
        await db.SaveChangesAsync(cancellationToken);

        var link = signer.Sign(id, MediaPart.Upload);
        return new UploadUrlResponse(id, MediaPaths.SignedUploadLink(id, link), DateTimeOffset.FromUnixTimeSeconds(link.Expires));
    }
}
