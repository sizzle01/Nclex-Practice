using System.Text;
using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Domain.Content;
using Nclex.Domain.Entities;

namespace Nclex.Application.Content.ImportQuestions;

/// <summary>
/// Imports the media an import file lists: reads each file from beside the import file, checks it
/// (<see cref="MediaRules"/>), stores it and writes its media_assets row. It also checks that every
/// mediaId the questions use exists, in the file or already imported.
/// </summary>
public sealed class MediaImporter(IAppDbContext db, IMediaStorage storage)
{
    /// <summary>Folder of every media key in storage.</summary>
    public const string KeyPrefix = "media/";

    /// <summary>
    /// Reads and checks the file's media and the questions' references, adding a problem to
    /// <paramref name="issues"/> for each thing wrong. Returns the media ready to write.
    /// </summary>
    public async Task<IReadOnlyList<PreparedMedia>> ValidateAsync(
        IReadOnlyList<ImportedMedia>? media,
        IReadOnlyList<MediaReference> references,
        IContentFiles files,
        List<ImportIssue> issues,
        CancellationToken cancellationToken)
    {
        var prepared = new List<PreparedMedia>();
        var seenIds = new HashSet<Guid>();
        for (var i = 0; i < (media?.Count ?? 0); i++)
        {
            var item = media![i];
            var path = $"media[{i}]";
            var before = issues.Count;
            if (item.Id == Guid.Empty)
            {
                issues.Add(new ImportIssue($"{path}.id", ImportIssueCodes.Required, "Every media asset needs a fixed id."));
            }
            else if (!seenIds.Add(item.Id))
            {
                issues.Add(new ImportIssue($"{path}.id", ImportIssueCodes.DuplicateId, $"Id {item.Id} is used more than once."));
            }

            issues.AddRange(MediaRules.Check(path, item.Kind, item.MimeType, item.AltText, item.CaptionsFile is not null)
                .Select(issue => new ImportIssue(issue.Path, issue.Code, issue.Message)));

            var content = await ReadAsync(files, item.File, $"{path}.file", issues, cancellationToken);
            var captions = item.CaptionsFile is null ? null : await ReadAsync(files, item.CaptionsFile, $"{path}.captionsFile", issues, cancellationToken);
            if (captions is not null && !IsWebVtt(captions))
            {
                issues.Add(new ImportIssue($"{path}.captionsFile", ImportIssueCodes.InvalidValue, "Captions must be a WebVTT file (it starts with \"WEBVTT\")."));
            }

            if (issues.Count == before && content is not null)
            {
                prepared.Add(new PreparedMedia(item.Id, content, captions, Describe(item, content.Length)));
            }
        }

        await CheckReferencesAsync(references, media ?? [], issues, cancellationToken);
        return prepared;
    }

    /// <summary>Stores the files and inserts or updates the rows. Runs inside the import's transaction.</summary>
    public async Task<ImportCounts> WriteAsync(IReadOnlyList<PreparedMedia> media, Guid createdBy, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var ids = media.Select(m => m.Id).ToList();
        var existing = await db.MediaAssets.Where(m => ids.Contains(m.Id)).ToDictionaryAsync(m => m.Id, cancellationToken);
        int created = 0, updated = 0, unchanged = 0;
        foreach (var item in media)
        {
            // Files are written every time: storage overwrites by key, so a changed file with the
            // same size and name still replaces the old one.
            await storage.SaveAsync(item.Details.StorageKey, item.Content, cancellationToken);
            if (item.Captions is not null)
            {
                await storage.SaveAsync(item.Details.CaptionsKey!, item.Captions, cancellationToken);
            }

            if (!existing.TryGetValue(item.Id, out var asset))
            {
                db.MediaAssets.Add(MediaAsset.Create(item.Id, item.Details, createdBy, now));
                created++;
            }
            else if (asset.Update(item.Details))
            {
                updated++;
            }
            else
            {
                unchanged++;
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        return new ImportCounts(created, updated, unchanged);
    }

    // A reference must name an asset in this file or one imported before, of the kind the exhibit shows.
    private async Task CheckReferencesAsync(
        IReadOnlyList<MediaReference> references,
        IReadOnlyList<ImportedMedia> media,
        List<ImportIssue> issues,
        CancellationToken cancellationToken)
    {
        var kinds = media.Where(m => m.Id != Guid.Empty).GroupBy(m => m.Id).ToDictionary(g => g.Key, g => g.First().Kind);
        var unknown = references.Select(r => r.MediaId).Where(id => !kinds.ContainsKey(id)).Distinct().ToList();
        foreach (var asset in await db.MediaAssets.AsNoTracking().Where(m => unknown.Contains(m.Id))
                     .Select(m => new { m.Id, m.Kind }).ToListAsync(cancellationToken))
        {
            kinds[asset.Id] = asset.Kind;
        }

        foreach (var reference in references)
        {
            if (!kinds.TryGetValue(reference.MediaId, out var kind))
            {
                issues.Add(new ImportIssue(reference.Path, ImportIssueCodes.UnknownMedia, "This mediaId is not in the file's media list and has not been imported before."));
            }
            else if (reference.Kind is not null && reference.Kind != kind)
            {
                issues.Add(new ImportIssue(reference.Path, ImportIssueCodes.InvalidValue, $"An {reference.Kind} exhibit can't show an {kind} asset."));
            }
        }
    }

    private static async Task<byte[]?> ReadAsync(IContentFiles files, string relativePath, string path, List<ImportIssue> issues, CancellationToken cancellationToken)
    {
        var content = string.IsNullOrWhiteSpace(relativePath) ? null : await files.ReadAsync(relativePath, cancellationToken);
        if (content is null || content.Length == 0)
        {
            issues.Add(new ImportIssue(path, ImportIssueCodes.FileNotFound, $"\"{relativePath}\" was not found next to the import file, or is empty."));
            return null;
        }

        return content;
    }

    private static bool IsWebVtt(byte[] captions)
    {
        var start = Encoding.UTF8.GetString(captions, 0, Math.Min(captions.Length, 16)).TrimStart('﻿');
        return start.StartsWith("WEBVTT", StringComparison.Ordinal);
    }

    private static MediaDetails Describe(ImportedMedia item, long bytes)
    {
        // Keys use the asset id, so two assets can never share a file and a re-import overwrites its own.
        var extension = Path.GetExtension(item.File).ToLowerInvariant();
        return new MediaDetails(
            item.Kind,
            $"{KeyPrefix}{item.Id:N}{extension}",
            item.MimeType,
            bytes,
            item.Width,
            item.Height,
            item.DurationMs,
            string.IsNullOrWhiteSpace(item.AltText) ? null : item.AltText.Trim(),
            item.CaptionsFile is null ? null : $"{KeyPrefix}{item.Id:N}.vtt");
    }
}

/// <summary>A checked media asset with its file contents, ready to store.</summary>
public sealed record PreparedMedia(Guid Id, byte[] Content, byte[]? Captions, MediaDetails Details);
