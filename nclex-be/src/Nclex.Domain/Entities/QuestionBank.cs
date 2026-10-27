namespace Nclex.Domain.Entities;

/// <summary>
/// A named collection of questions shown to candidates, e.g. "Pharmacology essentials".
/// Maps to <c>question_banks</c> (database.md section 2.4). The content importer loads banks
/// until managing them arrives with F14.
/// </summary>
public sealed class QuestionBank
{
    // For EF Core, which sets properties directly when loading rows.
    private QuestionBank()
    {
    }

    public Guid Id { get; private set; }

    public ExamTrack Track { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    /// <summary>Premium banks are locked for free accounts.</summary>
    public PlanTier AccessTier { get; private set; }

    public bool IsPublished { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    /// Creates a published bank. The caller chooses the id, so content files can refer to the same
    /// bank every time they are imported.
    /// </summary>
    public static QuestionBank CreatePublished(Guid id, ExamTrack track, string name, string? description, PlanTier accessTier, DateTimeOffset now) =>
        new()
        {
            Id = id,
            Track = track,
            Name = name,
            Description = description,
            AccessTier = accessTier,
            IsPublished = true,
            CreatedAt = now,
        };

    /// <summary>True when the bank already has these details, so an import has nothing to change.</summary>
    public bool HasDetails(string name, string? description, PlanTier accessTier) =>
        Name == name && Description == description && AccessTier == accessTier;

    public void ChangeDetails(string name, string? description, PlanTier accessTier)
    {
        Name = name;
        Description = description;
        AccessTier = accessTier;
    }
}
