using Nclex.Domain.Entities;
using Nclex.Domain.ItemTypes;

namespace Nclex.Application.Admin.Questions.Search;

/// <summary>Body of GET /admin/questions: one page of questions, in the cursor format of api.md section 1.</summary>
/// <param name="NextCursor">Pass as <c>cursor</c> for the next page; null on the last page.</param>
public sealed record QuestionSearchPage(IReadOnlyList<QuestionSummary> Items, string? NextCursor);

/// <summary>A question as the list shows it: its latest version, which is the one being worked on.</summary>
/// <param name="Status">active or retired.</param>
/// <param name="LatestStatus">Where the latest version is in the review workflow.</param>
/// <param name="PublishedVersion">The version number candidates get; null when none is published.</param>
/// <param name="UpdatedAt">When the latest version was created.</param>
public sealed record QuestionSummary(
    Guid Id,
    ExamTrack Track,
    QuestionStatus Status,
    PlanTier AccessTier,
    Guid LatestVersionId,
    int LatestVersion,
    VersionStatus LatestStatus,
    ItemType ItemType,
    string Stem,
    short ClientNeedsId,
    short BodySystemId,
    DifficultyTier Difficulty,
    Guid AuthorId,
    string AuthorName,
    int? PublishedVersion,
    DateTimeOffset UpdatedAt);
