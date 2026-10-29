using Nclex.Domain.Entities;
using Nclex.Domain.ItemTypes;

namespace Nclex.Application.Admin.Questions.Search;

/// <summary>Query string of GET /admin/questions (api.md section 8).</summary>
/// <param name="Track">RN or PN; both when left out.</param>
/// <param name="Status">
/// <c>published</c>: questions candidates can get (they have a current version). Any other status:
/// questions whose latest version has it, e.g. <c>in_review</c> for the reviewer queue.
/// </param>
/// <param name="ItemType">The latest version's item type.</param>
/// <param name="ClientNeedsId">The latest version's Client Needs subcategory.</param>
/// <param name="Q">Text in the latest version's stem, or a question id.</param>
/// <param name="QuestionStatus">active or retired; both when left out (added in F14 for set pickers).</param>
/// <param name="Limit">Page size, 1 to 100; 20 when left out.</param>
/// <param name="Cursor">The <c>nextCursor</c> of the previous page.</param>
public sealed record SearchQuestionsRequest(
    ExamTrack? Track = null,
    VersionStatus? Status = null,
    ItemType? ItemType = null,
    short? ClientNeedsId = null,
    string? Q = null,
    QuestionStatus? QuestionStatus = null,
    int? Limit = null,
    string? Cursor = null)
{
    public const int MaxQueryLength = 200;
}
