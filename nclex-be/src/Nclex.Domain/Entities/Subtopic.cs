namespace Nclex.Domain.Entities;

/// <summary>
/// A specific topic inside one Client Needs leaf, e.g. "Anticoagulant therapy and monitoring" under
/// Pharmacological and Parenteral Therapies. Result pages name these rather than the broad category,
/// so a candidate knows what to study. Maps to <c>subtopics</c> (database.md section 2.4).
/// </summary>
public sealed class Subtopic
{
    // For EF Core, which sets properties directly when loading rows.
    private Subtopic()
    {
    }

    public Guid Id { get; private set; }

    /// <summary>The Client Needs leaf it belongs to, which also decides its track.</summary>
    public short ClientNeedsId { get; private set; }

    /// <summary>Stable name for content files, unique within the leaf. It never changes after creation.</summary>
    public string Slug { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    /// <summary>Inactive subtopics stay on old questions but are no longer offered or recommended.</summary>
    public bool IsActive { get; private set; }

    public short SortOrder { get; private set; }

    public static Subtopic Create(short clientNeedsId, string slug, string name, short sortOrder, DateTimeOffset now) =>
        new()
        {
            Id = Guid.CreateVersion7(now),
            ClientNeedsId = clientNeedsId,
            Slug = slug,
            Name = name,
            IsActive = true,
            SortOrder = sortOrder,
        };

    public void ChangeDetails(string name, short sortOrder)
    {
        Name = name;
        SortOrder = sortOrder;
    }

    public void SetActive(bool isActive) => IsActive = isActive;
}
