using System.Text.Json.Serialization;
using Nclex.Domain.ItemTypes;

namespace Nclex.Application.Taxonomy.GetTaxonomy;

/// <summary>Body of GET /taxonomy: everything a question can be tagged with (api.md section 3).</summary>
public sealed record TaxonomyResponse(
    IReadOnlyList<ClientNeedsNode> ClientNeeds,
    IReadOnlyList<BodySystemInfo> BodySystems,
    IReadOnlyList<ItemTypeInfo> ItemTypes,
    IReadOnlyList<CjmmSkillInfo> CjmmSkills);

/// <summary>
/// A Client Needs category. Top-level categories with subcategories have <see cref="Children"/>;
/// leaves (what questions are tagged to) have weights instead.
/// </summary>
/// <param name="WeightMin">Lowest share of the exam, in percent; leaves only.</param>
/// <param name="WeightMax">Highest share of the exam, in percent; leaves only.</param>
public sealed record ClientNeedsNode(
    short Id,
    string Code,
    string Name,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] decimal? WeightMin,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] decimal? WeightMax,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] IReadOnlyList<ClientNeedsNode>? Children);

public sealed record BodySystemInfo(short Id, string Slug, string Name);

/// <param name="Available">True when the type has a module, so questions of that type can be served.</param>
public sealed record ItemTypeInfo(ItemType Type, string Label, bool Available);
