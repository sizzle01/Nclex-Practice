namespace Nclex.Domain.Entities;

/// <summary>
/// A named collection of questions shown to candidates, e.g. "Pharmacology essentials".
/// Maps to <c>question_banks</c> (database.md section 2.4). Managing banks arrives with F14.
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
}
