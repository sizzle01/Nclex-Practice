namespace Nclex.Domain.Entities;

/// <summary>
/// The stable identity of an item across edits. Its content lives in <see cref="QuestionVersion"/>
/// rows, and <see cref="CurrentVersionId"/> points at the one that is published. Maps to
/// <c>questions</c> (database.md section 2.4).
/// </summary>
public sealed class Question
{
    // For EF Core, which sets properties directly when loading rows.
    private Question()
    {
    }

    public Guid Id { get; private set; }

    public ExamTrack Track { get; private set; }

    public QuestionStatus Status { get; private set; }

    /// <summary>The published version; null until the first version is published.</summary>
    public Guid? CurrentVersionId { get; private set; }

    /// <summary>Premium questions are only served to premium accounts.</summary>
    public PlanTier AccessTier { get; private set; }

    public Guid? CreatedBy { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? RetiredAt { get; private set; }

    /// <summary>
    /// Creates an active question with no published version. The caller chooses the id, so content
    /// files can refer to the same question every time they are imported.
    /// </summary>
    public static Question Create(Guid id, ExamTrack track, PlanTier accessTier, Guid createdBy, DateTimeOffset now) =>
        new()
        {
            Id = id,
            Track = track,
            Status = QuestionStatus.Active,
            AccessTier = accessTier,
            CreatedBy = createdBy,
            CreatedAt = now,
        };

    /// <summary>
    /// Publishes <paramref name="version"/> and supersedes <paramref name="currentlyPublished"/>, which
    /// must be the version that was published until now (database.md section 2.4 rules). The caller
    /// saves both versions and this question in one transaction.
    /// </summary>
    public void Publish(QuestionVersion version, QuestionVersion? currentlyPublished, DateTimeOffset now)
    {
        if (version.QuestionId != Id)
        {
            throw new InvalidOperationException("The version belongs to a different question.");
        }

        if (currentlyPublished?.Id != CurrentVersionId)
        {
            throw new InvalidOperationException("The currently published version given is not this question's current version.");
        }

        currentlyPublished?.Supersede();
        version.Publish(now);
        CurrentVersionId = version.Id;
    }

    public void ChangeAccessTier(PlanTier accessTier) => AccessTier = accessTier;

    public bool IsRetired => Status == QuestionStatus.Retired;

    /// <summary>
    /// Takes the question out of future sets (api.md section 8). Attempts already started keep
    /// serving the version they resolved, so nobody loses an item halfway through.
    /// </summary>
    public void Retire(DateTimeOffset now)
    {
        if (IsRetired)
        {
            throw new InvalidOperationException($"Question {Id} is already retired.");
        }

        Status = QuestionStatus.Retired;
        RetiredAt = now;
    }
}
