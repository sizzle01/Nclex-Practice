namespace Nclex.Domain.Entities;

/// <summary>
/// A body system or content area used to tag questions, e.g. "Cardiovascular" or "Pharmacology".
/// Maps to <c>body_systems</c> (database.md section 2.3). Rows are seed data.
/// </summary>
public sealed class BodySystem
{
    // For EF Core, which sets properties directly when loading rows.
    private BodySystem()
    {
    }

    public short Id { get; private set; }

    /// <summary>Stable URL-friendly key, e.g. "renal-urinary". Content files refer to body systems by slug.</summary>
    public string Slug { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;
}
