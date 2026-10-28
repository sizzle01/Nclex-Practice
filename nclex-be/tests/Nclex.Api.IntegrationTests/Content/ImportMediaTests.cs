using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nclex.Api.IntegrationTests.Infrastructure;
using Nclex.Application.Content.ImportQuestions;
using Nclex.Domain.ItemTypes;
using Nclex.Infrastructure.Persistence;

namespace Nclex.Api.IntegrationTests.Content;

/// <summary>Media in import files (F11): stored files, media_assets rows, and the checks on both.</summary>
public sealed class ImportMediaTests(ApiFactory factory) : IDisposable
{
    private readonly CatalogContent _content = new(factory);
    private readonly MediaContent _media = new();

    [Fact]
    public async Task Media_are_stored_with_their_rows_and_importing_again_changes_nothing()
    {
        var first = await _content.ImportAsync(_media.File([_media.Image(), _media.Clip()]), _media.Files);
        var second = await _content.ImportAsync(_media.File([_media.Image(), _media.Clip()]), _media.Files);

        first.Issues.ShouldBeEmpty();
        first.Media.ShouldBe(new ImportCounts(2, 0, 0));
        second.Media.ShouldBe(new ImportCounts(0, 0, 2));

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var image = await db.MediaAssets.SingleAsync(m => m.Id == _media.ImageId, TestContext.Current.CancellationToken);
        image.ShouldSatisfyAllConditions(
            m => m.Kind.ShouldBe("image"),
            m => m.MimeType.ShouldBe("image/png"),
            m => m.Bytes.ShouldBe(MediaContent.Png.Length),
            m => m.AltText.ShouldBe("A red area over the sacrum"),
            m => m.StorageKey.ShouldBe($"media/{_media.ImageId:N}.png"),
            m => m.CaptionsKey.ShouldBeNull());
        (await File.ReadAllBytesAsync(Path.Combine(factory.MediaRoot, image.StorageKey), TestContext.Current.CancellationToken)).ShouldBe(MediaContent.Png);

        var clip = await db.MediaAssets.SingleAsync(m => m.Id == _media.AudioId, TestContext.Current.CancellationToken);
        clip.CaptionsKey.ShouldBe($"media/{_media.AudioId:N}.vtt");
        (await File.ReadAllBytesAsync(Path.Combine(factory.MediaRoot, clip.CaptionsKey!), TestContext.Current.CancellationToken)).ShouldBe(MediaContent.Captions);
    }

    [Fact]
    public async Task Changing_the_alt_text_updates_the_row()
    {
        await _content.ImportOkAsync(_media.File([_media.Image(), _media.Clip()]), _media.Files);

        var result = await _content.ImportAsync(_media.File([_media.Image(altText: "Stage 1 pressure injury"), _media.Clip()]), _media.Files);

        result.Media.ShouldBe(new ImportCounts(0, 1, 1));
    }

    [Fact]
    public async Task A_question_can_use_media_imported_by_an_earlier_file()
    {
        await _content.ImportOkAsync(_media.File([_media.Image(), _media.Clip()]), _media.Files);
        var laterQuestion = new MediaContent();

        var result = await _content.ImportAsync(laterQuestion.File([], [new { id = "photo", kind = "image", title = "Sacrum", mediaId = _media.ImageId }]));

        result.Issues.ShouldBeEmpty();
        laterQuestion.Dispose();
    }

    public static TheoryData<string, string, string> BrokenMedia => new()
    {
        { "no alt text", "media[0].altText", ValidationIssueCodes.Required },
        { "no captions", "media[1].captionsFile", ValidationIssueCodes.Required },
        { "MIME type of another kind", "media[0].mimeType", ValidationIssueCodes.Mismatch },
        { "missing file", "media[0].file", ImportIssueCodes.FileNotFound },
        { "path outside the folder", "media[0].file", ImportIssueCodes.FileNotFound },
        { "captions that are not WebVTT", "media[1].captionsFile", ImportIssueCodes.InvalidValue },
    };

    [Theory]
    [MemberData(nameof(BrokenMedia))]
    public async Task A_media_problem_writes_nothing(string problem, string path, string code)
    {
        File.WriteAllText(Path.Combine(_media.Folder, "media", "not-vtt.txt"), "just text");
        // A real file next to the import folder, so only the path rule can reject it.
        var outside = $"outside-{Guid.NewGuid():N}.png";
        File.WriteAllBytes(Path.Combine(Path.GetDirectoryName(_media.Folder)!, outside), MediaContent.Png);
        var (image, clip) = problem switch
        {
            "no alt text" => (_media.Image(altText: null), _media.Clip()),
            "no captions" => (_media.Image(), _media.Clip(captionsFile: null)),
            "MIME type of another kind" => (_media.Image(mimeType: "audio/wav"), _media.Clip()),
            "missing file" => (_media.Image(file: "media/missing.png"), _media.Clip()),
            "path outside the folder" => (_media.Image(file: $"../{outside}"), _media.Clip()),
            _ => (_media.Image(), _media.Clip(captionsFile: "media/not-vtt.txt")),
        };

        var result = await _content.ImportAsync(_media.File([image, clip]), _media.Files);

        File.Delete(Path.Combine(Path.GetDirectoryName(_media.Folder)!, outside));
        result.Issues.ShouldContain(i => i.Path == path && i.Code == code, problem);
        await ShouldNotExistAsync();
    }

    [Fact]
    public async Task An_exhibit_must_show_media_that_exist_and_of_its_own_kind()
    {
        var unknown = Guid.NewGuid();
        object[] exhibits =
        [
            new { id = "photo", kind = "image", title = "Sacrum", mediaId = unknown },
            new { id = "sound", kind = "audio", title = "Lung sounds", mediaId = _media.ImageId },
        ];

        var result = await _content.ImportAsync(_media.File([_media.Image()], exhibits), _media.Files);

        result.Issues.Select(i => (i.Path, i.Code)).ShouldBe(
        [
            ("questions[0].exhibits[0].mediaId", ImportIssueCodes.UnknownMedia),
            ("questions[0].exhibits[1].mediaId", ImportIssueCodes.InvalidValue),
        ], ignoreOrder: true);
        await ShouldNotExistAsync();
    }

    [Fact]
    public async Task A_malformed_exhibit_is_reported_with_its_path()
    {
        object[] exhibits =
        [
            new { id = "vitals", kind = "flowsheet", title = "Vital signs", table = new { columns = new[] { "08:00", "12:00" }, rows = new[] { new { label = "HR", cells = new[] { "80" } } } } },
            new { id = "note", kind = "chart", title = "Note" },
        ];

        var result = await _content.ImportAsync(_media.File([], exhibits));

        result.Issues.Select(i => (i.Path, i.Code)).ShouldBe(
        [
            ("questions[0].exhibits[0].table.rows[0].cells", ValidationIssueCodes.Mismatch),
            ("questions[0].exhibits[1].kind", ValidationIssueCodes.InvalidValue),
        ], ignoreOrder: true);
    }

    [Fact]
    public async Task A_graphic_option_must_use_known_media()
    {
        var question = CatalogContent.Question(_media.QuestionId);
        question["content"] = new { options = new[] { "a", "b", "c", "d" }.Select(o => new { id = o, text = $"Option {o}", mediaId = Guid.NewGuid() }) };

        var result = await _content.ImportAsync(CatalogContent.FileWith("RN", [question]));

        result.Issues.Count(i => i.Code == ImportIssueCodes.UnknownMedia).ShouldBe(4);
        result.Issues.ShouldContain(i => i.Path == "questions[0].content.options[0].mediaId");
    }

    public void Dispose() => _media.Dispose();

    private async Task ShouldNotExistAsync()
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        (await db.MediaAssets.AnyAsync(m => m.Id == _media.ImageId || m.Id == _media.AudioId, TestContext.Current.CancellationToken)).ShouldBeFalse();
        (await db.Questions.AnyAsync(q => q.Id == _media.QuestionId, TestContext.Current.CancellationToken)).ShouldBeFalse();
        File.Exists(Path.Combine(factory.MediaRoot, $"media/{_media.ImageId:N}.png")).ShouldBeFalse();
    }
}
