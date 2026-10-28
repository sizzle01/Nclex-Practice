using System.Text.Json;
using Nclex.Domain.ItemTypes;

namespace Nclex.Domain.Entities;

/// <summary>
/// One immutable revision of a question: its item payloads, tags and review state. Maps to
/// <c>question_versions</c> (database.md section 2.4). Attempts point at the version they served,
/// so editing a question never changes what a past attempt showed.
/// </summary>
public sealed class QuestionVersion
{
    /// <summary>The review note on versions loaded by the content importer instead of a reviewer.</summary>
    public const string ImportedReviewNote = "Imported";

    // For EF Core, which sets properties directly when loading rows.
    private QuestionVersion()
    {
    }

    public Guid Id { get; private set; }

    public Guid QuestionId { get; private set; }

    /// <summary>1, 2, 3... per question.</summary>
    public int Version { get; private set; }

    public VersionStatus Status { get; private set; }

    public ItemType ItemType { get; private set; }

    public string Stem { get; private set; } = string.Empty;

    /// <summary>Render payload (options, grid, blanks...). Safe to send to candidates.</summary>
    public JsonDocument Content { get; private set; } = null!;

    /// <summary>Never sent to candidates before the reveal rules allow it, and never logged.</summary>
    public JsonDocument AnswerKey { get; private set; } = null!;

    /// <summary>Hidden from candidates until the reveal.</summary>
    public JsonDocument Rationale { get; private set; } = null!;

    public string EducationalObjective { get; private set; } = string.Empty;

    /// <summary>A JSON array of exhibits; empty when there are none.</summary>
    public JsonDocument Exhibits { get; private set; } = null!;

    public short ClientNeedsId { get; private set; }

    public short BodySystemId { get; private set; }

    public CjmmSkill? CjmmSkill { get; private set; }

    public DifficultyTier Difficulty { get; private set; }

    /// <summary>Item response theory discrimination; for future analytics.</summary>
    public decimal? IrtA { get; private set; }

    /// <summary>Item response theory difficulty; used by CAT mode (architecture.md 5.8).</summary>
    public decimal? IrtB { get; private set; }

    /// <summary>Item response theory guessing; for future analytics.</summary>
    public decimal? IrtC { get; private set; }

    public int TimeAllotmentSec { get; private set; }

    public short MaxPoints { get; private set; }

    public Guid? CaseStudyId { get; private set; }

    /// <summary>1 to 6 within the case study; set exactly when <see cref="CaseStudyId"/> is.</summary>
    public short? CasePosition { get; private set; }

    public Guid AuthorId { get; private set; }

    public Guid? ReviewerId { get; private set; }

    public string? ReviewNotes { get; private set; }

    public DateTimeOffset? ReviewedAt { get; private set; }

    public DateTimeOffset? PublishedAt { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    /// Creates a version loaded by the content importer. The import file stands in for review, so the
    /// version starts approved, with no reviewer, ready for <see cref="Question.Publish"/>.
    /// </summary>
    public static QuestionVersion CreateImported(Guid questionId, int version, AuthoredItem item, Guid authorId, DateTimeOffset now) =>
        new()
        {
            Id = Guid.CreateVersion7(now),
            QuestionId = questionId,
            Version = version,
            Status = VersionStatus.Approved,
            ItemType = item.ItemType,
            Stem = item.Stem,
            Content = ToDocument(item.Content),
            AnswerKey = ToDocument(item.AnswerKey),
            Rationale = ToDocument(item.Rationale),
            EducationalObjective = item.EducationalObjective,
            Exhibits = ToDocument(item.Exhibits),
            ClientNeedsId = item.ClientNeedsId,
            BodySystemId = item.BodySystemId,
            CjmmSkill = item.CjmmSkill,
            Difficulty = item.Difficulty,
            TimeAllotmentSec = item.TimeAllotmentSec,
            MaxPoints = item.MaxPoints,
            CaseStudyId = item.Case?.CaseStudyId,
            CasePosition = item.Case?.Position,
            AuthorId = authorId,
            ReviewNotes = ImportedReviewNote,
            ReviewedAt = now,
            CreatedAt = now,
        };

    /// <summary>
    /// True when this version already holds exactly <paramref name="item"/>. JSON is compared by
    /// value, because Postgres jsonb does not keep the original key order or spacing.
    /// </summary>
    public bool Matches(AuthoredItem item) =>
        ItemType == item.ItemType
        && Stem == item.Stem
        && EducationalObjective == item.EducationalObjective
        && ClientNeedsId == item.ClientNeedsId
        && BodySystemId == item.BodySystemId
        && CjmmSkill == item.CjmmSkill
        && Difficulty == item.Difficulty
        && TimeAllotmentSec == item.TimeAllotmentSec
        && MaxPoints == item.MaxPoints
        && CaseStudyId == item.Case?.CaseStudyId
        && CasePosition == item.Case?.Position
        && JsonElement.DeepEquals(Content.RootElement, item.Content)
        && JsonElement.DeepEquals(AnswerKey.RootElement, item.AnswerKey)
        && JsonElement.DeepEquals(Rationale.RootElement, item.Rationale)
        && JsonElement.DeepEquals(Exhibits.RootElement, item.Exhibits);

    /// <summary>Makes this the version candidates see. Only an approved version can be published.</summary>
    internal void Publish(DateTimeOffset now)
    {
        if (Status != VersionStatus.Approved)
        {
            throw new InvalidOperationException($"Only an approved version can be published; this one is {Status}.");
        }

        Status = VersionStatus.Published;
        PublishedAt = now;
    }

    /// <summary>Retires this version from candidates because a newer one was published.</summary>
    internal void Supersede()
    {
        if (Status != VersionStatus.Published)
        {
            throw new InvalidOperationException($"Only the published version can be superseded; this one is {Status}.");
        }

        Status = VersionStatus.Superseded;
    }

    private static JsonDocument ToDocument(JsonElement element) => JsonDocument.Parse(element.GetRawText());
}
