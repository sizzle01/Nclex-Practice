using System.Buffers.Text;
using System.Globalization;
using System.Text;

namespace Nclex.Application.Catalog;

/// <summary>
/// The opaque <c>cursor</c> of a paged list (api.md section 1). It holds how many rows came before
/// the next page. Catalog lists are small and change rarely, so counting rows is simpler than a
/// keyset over their display order (sample first, then mode, then title). Clients must not read
/// it, which keeps the format free to change.
/// </summary>
public static class PageCursor
{
    private const string Prefix = "o:";

    public static string Encode(int offset) =>
        Base64Url.EncodeToString(Encoding.UTF8.GetBytes(Prefix + offset.ToString(CultureInfo.InvariantCulture)));

    /// <summary>Reads a cursor made by <see cref="Encode"/>; false for anything else.</summary>
    public static bool TryDecode(string cursor, out int offset)
    {
        offset = 0;
        byte[] bytes;
        try
        {
            bytes = Base64Url.DecodeFromChars(cursor);
        }
        catch (FormatException)
        {
            return false;
        }

        var text = Encoding.UTF8.GetString(bytes);
        return text.StartsWith(Prefix, StringComparison.Ordinal)
            && int.TryParse(text.AsSpan(Prefix.Length), NumberStyles.None, CultureInfo.InvariantCulture, out offset);
    }
}
