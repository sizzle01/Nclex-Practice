using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Application.Common;

namespace Nclex.Application.Media.GetMedia;

/// <summary>
/// GET /media/{id}: the asset's details with freshly signed links. Any signed-in user may ask;
/// media are only reachable through items already shown to them (premium gating is a follow-up).
/// </summary>
public sealed class GetMediaHandler(IAppDbContext db, IMediaUrlSigner signer)
{
    public async Task<MediaView> HandleAsync(Guid mediaId, CancellationToken cancellationToken)
    {
        var asset = await db.MediaAssets.AsNoTracking().SingleOrDefaultAsync(m => m.Id == mediaId, cancellationToken)
            ?? throw new AppException(MediaErrors.NotFound);

        var file = signer.Sign(asset.Id, MediaPart.File);
        return new MediaView(
            asset.Id,
            asset.Kind,
            MediaPaths.SignedLink(asset.Id, MediaPart.File, file),
            asset.CaptionsKey is null ? null : LinkTo(asset.Id, MediaPart.Captions),
            asset.TranscriptKey is null ? null : LinkTo(asset.Id, MediaPart.Transcript),
            asset.AltText,
            asset.Width,
            asset.Height,
            asset.DurationMs,
            DateTimeOffset.FromUnixTimeSeconds(file.Expires));
    }

    private string LinkTo(Guid mediaId, MediaPart part) => MediaPaths.SignedLink(mediaId, part, signer.Sign(mediaId, part));
}
