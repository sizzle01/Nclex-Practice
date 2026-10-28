using System.Text;
using System.Text.Json;
using Nclex.Application.Content.ImportQuestions;
using Nclex.Infrastructure.Content;

namespace Nclex.Api.IntegrationTests.Infrastructure;

/// <summary>
/// Builds import files with media for tests: a temporary folder holding the media files, and a
/// question whose exhibits show them. Each instance has fresh ids and its own folder.
/// </summary>
public sealed class MediaContent : IDisposable
{
    /// <summary>A tiny valid PNG (1×1 pixel).</summary>
    public static readonly byte[] Png = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAIAAACQd1PeAAAADElEQVR4nGP4z8AAAAMBAQDJ/pLvAAAAAElFTkSuQmCC");

    public static readonly byte[] Audio = [.. Encoding.ASCII.GetBytes("RIFF"), .. new byte[2000]];

    public static readonly byte[] Captions = Encoding.UTF8.GetBytes("WEBVTT\n\n00:00.000 --> 00:01.000\n[Crackles]\n");

    public static readonly byte[] Transcript = Encoding.UTF8.GetBytes("Fine crackles are heard at the end of each breath in — both bases.\n");

    public MediaContent()
    {
        Directory.CreateDirectory(Path.Combine(Folder, "media"));
        System.IO.File.WriteAllBytes(Path.Combine(Folder, "media", "image.png"), Png);
        System.IO.File.WriteAllBytes(Path.Combine(Folder, "media", "clip.wav"), Audio);
        System.IO.File.WriteAllBytes(Path.Combine(Folder, "media", "clip.vtt"), Captions);
        System.IO.File.WriteAllBytes(Path.Combine(Folder, "media", "clip.txt"), Transcript);
    }

    public string Folder { get; } = Path.Combine(Path.GetTempPath(), $"nclex-import-{Guid.NewGuid():N}");

    public Guid ImageId { get; } = Guid.NewGuid();

    public Guid AudioId { get; } = Guid.NewGuid();

    public Guid QuestionId { get; } = Guid.NewGuid();

    public IContentFiles Files => new DirectoryContentFiles(Folder);

    /// <summary>The media list entry for the image; pass changes to break or alter it.</summary>
    public Dictionary<string, object?> Image(string? altText = "A red area over the sacrum", string file = "media/image.png", string mimeType = "image/png") => new()
    {
        ["id"] = ImageId,
        ["kind"] = "image",
        ["file"] = file,
        ["mimeType"] = mimeType,
        ["altText"] = altText,
        ["width"] = 1,
        ["height"] = 1,
    };

    public Dictionary<string, object?> Clip(string? captionsFile = "media/clip.vtt", string? transcriptFile = "media/clip.txt") => new()
    {
        ["id"] = AudioId,
        ["kind"] = "audio",
        ["file"] = "media/clip.wav",
        ["mimeType"] = "audio/wav",
        ["captionsFile"] = captionsFile,
        ["transcriptFile"] = transcriptFile,
        ["durationMs"] = 1000,
    };

    /// <summary>Exhibits showing both media files, plus a flowsheet.</summary>
    public object[] Exhibits() =>
    [
        new { id = "vitals", kind = "flowsheet", title = "Vital signs", table = new { columns = new[] { "08:00", "12:00" }, rows = new[] { new { label = "HR", cells = new[] { "80", "96" } } } } },
        new { id = "photo", kind = "image", title = "Sacrum", mediaId = ImageId },
        new { id = "sound", kind = "audio", title = "Lung sounds", mediaId = AudioId },
    ];

    /// <summary>An RN file with one question showing <paramref name="exhibits"/>, and <paramref name="media"/>.</summary>
    public QuestionImportFile File(IEnumerable<object?> media, object[]? exhibits = null, IEnumerable<object>? testSets = null)
    {
        var question = CatalogContent.Question(QuestionId);
        question["exhibits"] = exhibits ?? Exhibits();
        return QuestionImportFile.Parse(JsonSerializer.Serialize(new { track = "RN", questions = new[] { question }, media, testSets = testSets ?? [] }));
    }

    public void Dispose()
    {
        if (Directory.Exists(Folder))
        {
            Directory.Delete(Folder, recursive: true);
        }
    }
}
