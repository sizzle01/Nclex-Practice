using Nclex.Application.Catalog;

namespace Nclex.Application.Admin;

/// <summary>Page size and cursor rules shared by the admin lists (api.md section 1, Pagination).</summary>
public static class AdminPaging
{
    public const int DefaultLimit = 20;
    public const int MaxLimit = 100;

    /// <summary>The page size to use, and how many rows to skip for <paramref name="cursor"/>.</summary>
    public static (int Limit, int Offset) Read(int? limit, string? cursor) =>
        (limit ?? DefaultLimit, cursor is not null && PageCursor.TryDecode(cursor, out var offset) ? offset : 0);

    /// <summary>
    /// Trims the extra row that was fetched to see whether another page exists, and gives the cursor for it.
    /// </summary>
    public static (IReadOnlyList<T> Items, string? NextCursor) Page<T>(IReadOnlyList<T> rows, int limit, int offset) =>
        rows.Count > limit
            ? ([.. rows.Take(limit)], PageCursor.Encode(offset + limit))
            : (rows, null);
}

