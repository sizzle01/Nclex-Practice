using Nclex.Domain.Flags;

namespace Nclex.Application.Flags.Moderation;

/// <summary>Query string of GET /admin/flags (api.md section 8).</summary>
/// <param name="Status">Which reports to list; open when left out.</param>
/// <param name="Cursor">The <c>nextCursor</c> of the previous page.</param>
/// <param name="Limit">Page size, 1 to 50; 20 when left out.</param>
public sealed record ListFlagQueueRequest(FlagStatus? Status = null, string? Cursor = null, int? Limit = null)
{
    public const int DefaultLimit = 20;
    public const int MaxLimit = 50;
}
