using Nclex.Application.Attempts.GetItem;

namespace Nclex.Application.Attempts.Review;

/// <summary>
/// Body of GET /attempts/{id}/review: items in the shape of the item endpoint, each with its
/// reveal, in the cursor format of api.md section 1.
/// </summary>
/// <param name="NextCursor">Pass as <c>cursor</c> to get the next page; null on the last page.</param>
public sealed record ReviewPage(IReadOnlyList<CandidateItemView> Items, string? NextCursor);
