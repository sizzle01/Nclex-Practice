using Nclex.Application.Catalog;

namespace Nclex.Application.Tests.Catalog;

public sealed class PageCursorTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(20)]
    [InlineData(int.MaxValue)]
    public void A_cursor_reads_back_as_the_offset_it_was_made_from(int offset)
    {
        PageCursor.TryDecode(PageCursor.Encode(offset), out var decoded).ShouldBeTrue();

        decoded.ShouldBe(offset);
    }

    [Theory]
    [InlineData("")]
    [InlineData("%%%")]
    [InlineData("bzotMQ")] // "o:-1": negative offsets are not cursors
    public void Anything_else_is_not_a_cursor(string cursor) =>
        PageCursor.TryDecode(cursor, out _).ShouldBeFalse();
}
