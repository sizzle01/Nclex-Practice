namespace Nclex.Domain.Entities;

/// <summary>
/// A node of the NCLEX test plan's Client Needs tree for one track, e.g. "Safe and Effective Care
/// Environment" with child "Management of Care". Maps to <c>client_needs_categories</c>
/// (database.md section 2.3). Rows are seed data; the app only reads them.
/// </summary>
/// <remarks>
/// Questions are always tagged to a leaf: a subcategory, or a category that has no subcategories
/// (e.g. Psychosocial Integrity). Weights are the test plan's percentage range and are set on leaves.
/// </remarks>
public sealed class ClientNeedsCategory
{
    // For EF Core, which sets properties directly when loading rows.
    private ClientNeedsCategory()
    {
    }

    public short Id { get; private set; }

    public ExamTrack Track { get; private set; }

    /// <summary>The category this subcategory belongs to; null for a top-level category.</summary>
    public short? ParentId { get; private set; }

    /// <summary>Stable code, unique per track, e.g. "SECE" or "SECE.MOC".</summary>
    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    /// <summary>Lowest share of the exam, in percent (e.g. 15.00).</summary>
    public decimal? WeightMin { get; private set; }

    /// <summary>Highest share of the exam, in percent (e.g. 21.00).</summary>
    public decimal? WeightMax { get; private set; }

    public short SortOrder { get; private set; }
}
