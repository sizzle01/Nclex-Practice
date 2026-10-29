using System.Text.Json;
using Nclex.Domain.Content;
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
    /// Creates a draft written in the admin console (F14). It stays editable until the author
    /// submits it for review.
    /// </summary>
    public static QuestionVersion CreateDraft(Guid questionId, int version, AuthoredItem item, Guid authorId, DateTimeOffset now)
    {
        var draft = new QuestionVersion
        {
            Id = Guid.CreateVersion7(now),
            QuestionId = questionId,
            Version = version,
            Status = VersionStatus.Draft,
            AuthorId = authorId,
            CreatedAt = now,
        };
        draft.Apply(item);
        return draft;
    }

    /// <summary>
    /// A new draft with this version's content, tags and case placement, numbered
    /// <paramref name="version"/>. Whoever asks for it becomes its author. Review fields start empty.
    /// </summary>
    public QuestionVersion CloneAsDraft(int version, Guid authorId, DateTimeOffset now) =>
        CreateDraft(QuestionId, version, ToAuthoredItem(), authorId, now);

    /// <summary>What this version holds, in the shape authors write.</summary>
    public AuthoredItem ToAuthoredItem() => new(
        ItemType,
        Stem,
        Content.RootElement.Clone(),
        AnswerKey.RootElement.Clone(),
        Rationale.RootElement.Clone(),
        EducationalObjective,
        Exhibits.RootElement.Clone(),
        ClientNeedsId,
        BodySystemId,
        CjmmSkill,
        Difficulty,
        TimeAllotmentSec,
        MaxPoints,
        CaseStudyId is { } caseId && CasePosition is { } position ? new CasePlacement(caseId, position) : null);

    /// <summary>
    /// Replaces the draft's content and tags. Only drafts change: once a version leaves draft it is
    /// immutable (database.md 2.4), so a past attempt always shows what the candidate saw.
    /// </summary>
    public void EditDraft(AuthoredItem item)
    {
        Ensure(VersionAction.Edit, AuthorId);
        Apply(item);
    }

    /// <summary>draft → in_review: the author asks a reviewer to check it.</summary>
    public void SubmitForReview()
    {
        Ensure(VersionAction.SubmitForReview, AuthorId);
        Status = VersionStatus.InReview;
    }

    /// <summary>in_review → approved. The reviewer must not be the author (database.md 2.4).</summary>
    public void Approve(Guid reviewerId, string? notes, DateTimeOffset now) =>
        Review(VersionAction.Approve, reviewerId, notes, now);

    /// <summary>in_review → rejected, with notes telling the author what to fix in a new version.</summary>
    public void Reject(Guid reviewerId, string notes, DateTimeOffset now) =>
        Review(VersionAction.Reject, reviewerId, notes, now);

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

    private void Review(VersionAction action, Guid reviewerId, string? notes, DateTimeOffset now)
    {
        Ensure(action, reviewerId);
        Status = VersionWorkflow.To(action);
        ReviewerId = reviewerId;
        ReviewNotes = notes;
        ReviewedAt = now;
    }

    // The application checks the workflow first and answers with an error; reaching here is a bug.
    private void Ensure(VersionAction action, Guid actorId)
    {
        if (VersionWorkflow.Check(action, Status, AuthorId, actorId) is { } refusal)
        {
            throw new InvalidOperationException($"Cannot {action} version {Id} ({Status}): {refusal}.");
        }
    }

    private void Apply(AuthoredItem item)
    {
        ItemType = item.ItemType;
        Stem = item.Stem;
        Content = ToDocument(item.Content);
        AnswerKey = ToDocument(item.AnswerKey);
        Rationale = ToDocument(item.Rationale);
        EducationalObjective = item.EducationalObjective;
        Exhibits = ToDocument(item.Exhibits);
        ClientNeedsId = item.ClientNeedsId;
        BodySystemId = item.BodySystemId;
        CjmmSkill = item.CjmmSkill;
        Difficulty = item.Difficulty;
        TimeAllotmentSec = item.TimeAllotmentSec;
        MaxPoints = item.MaxPoints;
        CaseStudyId = item.Case?.CaseStudyId;
        CasePosition = item.Case?.Position;
    }

    private static JsonDocument ToDocument(JsonElement element) => JsonDocument.Parse(element.GetRawText());
}
