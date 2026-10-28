using Nclex.Domain.Content;
using Nclex.Domain.ItemTypes;

namespace Nclex.Domain.Tests.Content;

public sealed class MediaRulesTests
{
    [Theory]
    [InlineData("image", "image/png", "Red area over the sacrum", false)]
    [InlineData("audio", "audio/wav", null, true)]
    [InlineData("video", "video/mp4", null, true)]
    public void A_complete_asset_passes(string kind, string mimeType, string? altText, bool hasCaptions)
    {
        MediaRules.Check("media[0]", kind, mimeType, altText, hasCaptions).ShouldBeEmpty();
    }

    [Theory]
    [InlineData("image", "image/png", " ", false, "media[0].altText", ValidationIssueCodes.Required)]
    [InlineData("audio", "audio/wav", null, false, "media[0].captionsFile", ValidationIssueCodes.Required)]
    [InlineData("video", "video/mp4", null, false, "media[0].captionsFile", ValidationIssueCodes.Required)]
    [InlineData("image", "audio/wav", "Alt", false, "media[0].mimeType", ValidationIssueCodes.Mismatch)]
    [InlineData("document", "application/pdf", null, false, "media[0].kind", ValidationIssueCodes.InvalidValue)]
    public void An_incomplete_asset_is_reported(string kind, string mimeType, string? altText, bool hasCaptions, string path, string code)
    {
        MediaRules.Check("media[0]", kind, mimeType, altText, hasCaptions)
            .ShouldContain(i => i.Path == path && i.Code == code);
    }
}
