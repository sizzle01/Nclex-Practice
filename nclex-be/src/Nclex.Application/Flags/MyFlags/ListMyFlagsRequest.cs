namespace Nclex.Application.Flags.MyFlags;

/// <summary>Query string of GET /me/flags (api.md section 7).</summary>
/// <param name="Cursor">The <c>nextCursor</c> of the previous page.</param>
/// <param name="Limit">Page size, 1 to 50; 20 when left out.</param>
public sealed record ListMyFlagsRequest(string? Cursor = null, int? Limit = null)
{
    public const int DefaultLimit = 20;
    public const int MaxLimit = 50;
}
