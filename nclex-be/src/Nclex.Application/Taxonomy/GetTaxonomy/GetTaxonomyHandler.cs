using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Domain.Entities;
using Nclex.Domain.ItemTypes;

namespace Nclex.Application.Taxonomy.GetTaxonomy;

/// <summary>GET /taxonomy: the tag tree for one track, used by the test builder and authoring.</summary>
public sealed class GetTaxonomyHandler(IAppDbContext db, ItemTypeRegistry itemTypes)
{
    public async Task<TaxonomyResponse> HandleAsync(GetTaxonomyRequest request, CancellationToken cancellationToken)
    {
        var track = request.Track!.Value;
        var categories = await db.ClientNeedsCategories.AsNoTracking()
            .Where(c => c.Track == track)
            .OrderBy(c => c.SortOrder).ThenBy(c => c.Id)
            .ToListAsync(cancellationToken);
        var bodySystems = await db.BodySystems.AsNoTracking()
            .OrderBy(b => b.Id)
            .Select(b => new BodySystemInfo(b.Id, b.Slug, b.Name))
            .ToListAsync(cancellationToken);

        return new TaxonomyResponse(
            BuildTree(categories, parentId: null),
            bodySystems,
            [.. ItemTypeCatalog.All.Select(d => new ItemTypeInfo(d.Type, d.Label, itemTypes.IsAvailable(d.Type)))],
            CjmmSkillCatalog.All);
    }

    private static List<ClientNeedsNode> BuildTree(List<ClientNeedsCategory> categories, short? parentId) =>
        [.. categories
            .Where(c => c.ParentId == parentId)
            .Select(c =>
            {
                var children = BuildTree(categories, c.Id);
                return new ClientNeedsNode(c.Id, c.Code, c.Name, c.WeightMin, c.WeightMax, children.Count > 0 ? children : null);
            })];
}
