using System.Text;
using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Application.Common;
using Nclex.Application.Common.Validation;
using Nclex.Domain.Audit;
using Nclex.Domain.Content;

namespace Nclex.Application.Admin.Media;

/// <summary>
/// PATCH /admin/media/{id}: alt text for images, captions and a transcript for audio and video
/// (api.md section 8; interfaces.md section 4, Media). Captions and transcripts are small text
/// files, so they come in the body and are stored beside the asset.
/// </summary>
public sealed class UpdateMediaHandler(IAppDbContext db, IMediaStorage storage, ICurrentUser currentUser, IAuditLog audit)
{
    public async Task<AdminMediaView> HandleAsync(Guid mediaId, UpdateMediaRequest request, CancellationToken cancellationToken)
    {
        var asset = await db.MediaAssets.SingleOrDefaultAsync(m => m.Id == mediaId, cancellationToken)
            ?? throw new AppException(AdminErrors.MediaNotFound);
        if (asset.Kind == "image" && (request.Captions is not null || request.Transcript is not null))
        {
            throw AppException.ForField(request.Captions is not null ? "captions" : "transcript", ValidationErrorCodes.InvalidValue);
        }

        var details = asset.Details;
        if (request.AltText is not null)
        {
            details = details with { AltText = request.AltText.Trim().Length == 0 ? null : request.AltText.Trim() };
        }

        if (request.Captions is not null)
        {
            var key = MediaUploadRules.CaptionsKey(asset.Id);
            await storage.SaveAsync(key, Encoding.UTF8.GetBytes(request.Captions), cancellationToken);
            details = details with { CaptionsKey = key };
        }

        if (request.Transcript is not null)
        {
            var key = MediaUploadRules.TranscriptKey(asset.Id);
            await storage.SaveAsync(key, Encoding.UTF8.GetBytes(request.Transcript), cancellationToken);
            details = details with { TranscriptKey = key };
        }

        asset.Update(details);
        audit.RecordEntityEvent(AuditActions.MediaUpdated, currentUser.UserId, AuditActions.MediaEntity, asset.Id.ToString(), new
        {
            altText = request.AltText is not null,
            captions = request.Captions is not null,
            transcript = request.Transcript is not null,
        });
        await db.SaveChangesAsync(cancellationToken);

        await using var file = await storage.OpenReadAsync(asset.StorageKey, cancellationToken);
        return new AdminMediaView(asset.Id, asset.Kind, asset.MimeType, asset.Bytes, asset.AltText, file is not null, asset.CaptionsKey is not null, asset.TranscriptKey is not null);
    }
}
