namespace Nclex.Application.Attempts.ListAttempts;

/// <summary>Query string of GET /attempts (api.md section 4).</summary>
/// <param name="Status">Comma-separated statuses, e.g. "in_progress,paused"; drafts when left out.</param>
/// <param name="Limit">Page size, 1 to 50; 20 when left out.</param>
/// <param name="Cursor">The <c>nextCursor</c> of the previous page.</param>
public sealed record ListAttemptsRequest(string? Status = null, int? Limit = null, string? Cursor = null)
{
    public const int DefaultLimit = 20;
    public const int MaxLimit = 50;
}
