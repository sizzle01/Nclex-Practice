using Nclex.Application.Flags;

namespace Nclex.Application.Tests.Flags;

public sealed class StemPreviewTests
{
    [Fact]
    public void A_short_stem_is_shown_whole() =>
        StemPreview.From("  Which client should the nurse see first?  ").ShouldBe("Which client should the nurse see first?");

    [Fact]
    public void A_long_stem_is_cut_with_an_ellipsis()
    {
        var preview = StemPreview.From(new string('a', 300));

        preview.Length.ShouldBe(StemPreview.MaxLength);
        preview.ShouldEndWith("…");
    }
}
