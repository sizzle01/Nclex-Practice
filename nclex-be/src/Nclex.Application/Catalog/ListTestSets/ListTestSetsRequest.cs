using Nclex.Domain.Entities;

namespace Nclex.Application.Catalog.ListTestSets;

/// <summary>Query string of GET /test-sets (api.md section 3).</summary>
/// <param name="Track">RN or PN; the user's own track when left out.</param>
/// <param name="Kind">Only "prebuilt" for now; custom sets are listed from F07.</param>
/// <param name="Limit">Page size, 1 to 50; 20 when left out.</param>
/// <param name="Cursor">The <c>nextCursor</c> of the previous page.</param>
public sealed record ListTestSetsRequest(
    ExamTrack? Track = null,
    TestSetKind? Kind = null,
    int? Limit = null,
    string? Cursor = null)
{
    public const int DefaultLimit = 20;
    public const int MaxLimit = 50;
}
