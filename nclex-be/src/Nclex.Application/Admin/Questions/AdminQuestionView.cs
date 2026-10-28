using System.Text.Json;
using Nclex.Domain.Entities;
using Nclex.Domain.ItemTypes;

namespace Nclex.Application.Admin.Questions;

/// <summary>
/// GET /admin/questions/{id}: a question with every version, newest first, including keys and
/// rationales. Staff only (author, reviewer, admin), never a candidate (CLAUDE.md, Security).
/// </summary>
/// <param name="CurrentVersionId">The published version candidates get; null until one is published.</param>
/// <param name="RetiredAt">Set when the question was taken out of future sets.</param>
/// <param name="Versions">Newest first; the first is the one an author would work on.</param>
public sealed record AdminQuestionView(
    Guid Id,
    ExamTrack Track,
    QuestionStatus Status,
    PlanTier AccessTier,
    Guid? CurrentVersionId,
    DateTimeOffset CreatedAt,
    DateTimeOffset? RetiredAt,
    IReadOnlyList<AdminVersionView> Versions);

/// <summary>One version with its full payloads and review trail.</summary>
/// <param name="AuthorName">The author's display name, or email when they have none.</param>
/// <param name="ReviewerName">Who approved or rejected it, if anyone has.</param>
/// <param name="ReviewNotes">The reviewer's notes; on a rejected version, what to fix.</param>
public sealed record AdminVersionView(
    Guid Id,
    int Version,
    VersionStatus Status,
    ItemType ItemType,
    string Stem,
    JsonElement Content,
    JsonElement AnswerKey,
    JsonElement Rationale,
    string EducationalObjective,
    JsonElement Exhibits,
    short ClientNeedsId,
    short BodySystemId,
    CjmmSkill? CjmmSkill,
    DifficultyTier Difficulty,
    int TimeAllotmentSec,
    short MaxPoints,
    Guid? CaseStudyId,
    short? CasePosition,
    Guid AuthorId,
    string AuthorName,
    Guid? ReviewerId,
    string? ReviewerName,
    string? ReviewNotes,
    DateTimeOffset? ReviewedAt,
    DateTimeOffset? PublishedAt,
    DateTimeOffset CreatedAt);
