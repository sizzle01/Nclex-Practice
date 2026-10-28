namespace Nclex.Application.Attempts.Review;

/// <summary>Query string of GET /attempts/{id}/review (api.md section 4).</summary>
/// <param name="Filter">All items when left out.</param>
/// <param name="Position">Start the list at this position, e.g. to open the review at one item.</param>
/// <param name="Limit">Page size, 1 to 50; 20 when left out.</param>
/// <param name="Cursor">The <c>nextCursor</c> of the previous page.</param>
public sealed record GetReviewRequest(
    ReviewFilter? Filter = null,
    int? Position = null,
    int? Limit = null,
    string? Cursor = null)
{
    public const int DefaultLimit = 20;
    public const int MaxLimit = 50;
}
