using Nclex.Application.Notifications.Feed;

namespace Nclex.Application.Tests.Notifications;

public sealed class FeedCursorTests
{
    [Fact]
    public void A_cursor_reads_back_as_the_row_it_was_made_from()
    {
        var createdAt = new DateTimeOffset(2026, 10, 28, 9, 30, 15, 123, TimeSpan.Zero).AddTicks(4560);
        var id = Guid.NewGuid();

        FeedCursor.TryDecode(FeedCursor.Encode(createdAt, id), out var readAt, out var readId).ShouldBeTrue();

        (readAt, readId).ShouldBe((createdAt, id));
    }

    [Theory]
    [InlineData("")]
    [InlineData("not base64!")]
    [InlineData("bzoyMA")]          // an offset cursor from the catalog ("o:20")
    [InlineData("azp4OnNvbWV0aGluZw")] // "k:x:something"
    public void Anything_else_is_not_a_cursor(string cursor)
    {
        FeedCursor.TryDecode(cursor, out _, out _).ShouldBeFalse();
    }
}
