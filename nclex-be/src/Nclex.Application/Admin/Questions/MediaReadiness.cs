using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Application.Common;
using Nclex.Domain.Content;
using Nclex.Domain.Entities;
using Nclex.Domain.ItemTypes;

namespace Nclex.Application.Admin.Questions;

/// <summary>
/// Before a version goes to review or to candidates, every file it shows must be uploaded and
/// accessible: alt text for images, captions and a transcript for audio and video (database.md 2.4,
/// media_assets; interfaces.md section 4). Drafts may refer to media still being described.
/// </summary>
public sealed class MediaReadiness(IAppDbContext db, IMediaStorage storage)
{
    /// <summary>Throws 400 VALIDATION_FAILED listing each media problem of <paramref name="version"/>.</summary>
    public async Task EnsureReadyAsync(QuestionVersion version, CancellationToken cancellationToken)
    {
        var references = ExhibitRules.Check(version.Exhibits.RootElement).Media
            .Concat(ExhibitRules.MediaInContent(version.Content.RootElement))
            .ToList();
        if (references.Count == 0)
        {
            return;
        }

        var ids = references.Select(r => r.MediaId).Distinct().ToList();
        var assets = await db.MediaAssets.AsNoTracking().Where(m => ids.Contains(m.Id)).ToDictionaryAsync(m => m.Id, cancellationToken);
        var issues = new List<ValidationIssue>();
        foreach (var reference in references)
        {
            if (!assets.TryGetValue(reference.MediaId, out var asset))
            {
                issues.Add(new ValidationIssue(reference.Path, ValidationIssueCodes.UnknownId, $"Media {reference.MediaId} does not exist."));
                continue;
            }

            issues.AddRange(MediaRules.Check(reference.Path, asset.Kind, asset.MimeType, asset.AltText, asset.CaptionsKey is not null, asset.TranscriptKey is not null));
            if (!await IsUploadedAsync(asset, cancellationToken))
            {
                issues.Add(new ValidationIssue(reference.Path, ValidationIssueCodes.Required, "The file was never uploaded. Upload it again."));
            }
        }

        if (issues.Count > 0)
        {
            throw AuthoringValidator.Invalid(issues);
        }
    }

    private async Task<bool> IsUploadedAsync(MediaAsset asset, CancellationToken cancellationToken)
    {
        await using var stream = await storage.OpenReadAsync(asset.StorageKey, cancellationToken);
        return stream is not null;
    }
}
