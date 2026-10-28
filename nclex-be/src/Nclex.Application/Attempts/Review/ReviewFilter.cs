namespace Nclex.Application.Attempts.Review;

/// <summary>Which items the review page lists (api.md, GET /attempts/{id}/review <c>filter</c>).</summary>
public enum ReviewFilter
{
    All,

    /// <summary>Items that did not earn full marks, including blanks and partial credit.</summary>
    Incorrect,

    /// <summary>Items the candidate marked for review.</summary>
    Flagged,
}
