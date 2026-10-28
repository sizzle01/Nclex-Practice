using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Domain.Entities;
using Nclex.Domain.Grading;
using Nclex.Domain.ItemTypes;

namespace Nclex.Application.Attempts.Results;

/// <summary>
/// Display names for breakdown keys. Names are looked up when a result is read rather than stored,
/// so renaming a category in the taxonomy shows up on old results too.
/// </summary>
public sealed class DimensionNames(IAppDbContext db)
{
    public async Task<DimensionNameLookup> ForTrackAsync(ExamTrack track, CancellationToken cancellationToken)
    {
        var categories = await db.ClientNeedsCategories.AsNoTracking()
            .Where(c => c.Track == track)
            .ToDictionaryAsync(c => c.Code, c => c.Name, StringComparer.Ordinal, cancellationToken);
        var bodySystems = await db.BodySystems.AsNoTracking()
            .ToDictionaryAsync(b => b.Slug, b => b.Name, StringComparer.Ordinal, cancellationToken);
        return new DimensionNameLookup(categories, bodySystems);
    }
}

/// <summary>Names the keys of every <see cref="ScoreDimension"/>; falls back to the key itself.</summary>
public sealed class DimensionNameLookup(
    IReadOnlyDictionary<string, string> clientNeeds,
    IReadOnlyDictionary<string, string> bodySystems)
{
    private static readonly Dictionary<string, string> ItemTypeLabels =
        ItemTypeCatalog.All.ToDictionary(d => DimensionKeys.For(d.Type), d => d.Label, StringComparer.Ordinal);

    public string Name(ScoreDimension dimension, string key) => dimension switch
    {
        ScoreDimension.ClientNeeds or ScoreDimension.ClientNeedsParent => clientNeeds.GetValueOrDefault(key, key),
        ScoreDimension.BodySystem => bodySystems.GetValueOrDefault(key, key),
        ScoreDimension.ItemType => ItemTypeLabels.GetValueOrDefault(key, key),
        // "take_action" reads as "Take action", the wording of the NCSBN model.
        _ => char.ToUpperInvariant(key[0]) + key[1..].Replace('_', ' '),
    };
}
