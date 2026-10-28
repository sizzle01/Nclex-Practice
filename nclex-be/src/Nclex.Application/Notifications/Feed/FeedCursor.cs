using System.Buffers.Text;
using System.Globalization;
using System.Text;

namespace Nclex.Application.Notifications.Feed;

/// <summary>
/// The opaque <c>cursor</c> of the feed (api.md section 1): the position of the last row sent.
/// A keyset rather than an offset, because new notifications arrive at the top while the user
/// pages down, and an offset would then repeat rows.
/// </summary>
public static class FeedCursor
{
    private const string Prefix = "k:";

    public static string Encode(DateTimeOffset createdAt, Guid id) =>
        Base64Url.EncodeToString(Encoding.UTF8.GetBytes(
            $"{Prefix}{createdAt.UtcTicks.ToString(CultureInfo.InvariantCulture)}:{id:N}"));

    /// <summary>Reads a cursor made by <see cref="Encode"/>; false for anything else.</summary>
    public static bool TryDecode(string cursor, out DateTimeOffset createdAt, out Guid id)
    {
        (createdAt, id) = (default, default);
        byte[] bytes;
        try
        {
            bytes = Base64Url.DecodeFromChars(cursor);
        }
        catch (FormatException)
        {
            return false;
        }

        var parts = Encoding.UTF8.GetString(bytes).Split(':');
        if (parts is not ["k", var ticksText, var idText]
            || !long.TryParse(ticksText, NumberStyles.None, CultureInfo.InvariantCulture, out var ticks)
            || ticks > DateTimeOffset.MaxValue.UtcTicks
            || !Guid.TryParseExact(idText, "N", out id))
        {
            return false;
        }

        createdAt = new DateTimeOffset(ticks, TimeSpan.Zero);
        return true;
    }
}
