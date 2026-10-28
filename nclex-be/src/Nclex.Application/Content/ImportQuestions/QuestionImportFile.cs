using System.Text.Json;
using Nclex.Domain.Entities;
using Nclex.Domain.ItemTypes;

namespace Nclex.Application.Content.ImportQuestions;

/// <summary>
/// A JSON file of questions for the content importer, optionally with the banks and curated test
/// sets built from them. Everything carries a fixed id, so importing the same file again updates
/// those rows instead of adding copies.
/// </summary>
/// <param name="Track">The exam track of every question, bank and set in the file.</param>
/// <param name="Questions">The questions to publish.</param>
/// <param name="Notice">Free text for people reading the file, e.g. that it is sample content. Not stored.</param>
/// <param name="Banks">Question banks to publish; none by default.</param>
/// <param name="TestSets">Curated (prebuilt) test sets to publish; none by default.</param>
/// <param name="Media">Images, audio and video that the questions' exhibits and options use; none by default.</param>
public sealed record QuestionImportFile(
    ExamTrack Track,
    IReadOnlyList<ImportedQuestion> Questions,
    string? Notice = null,
    IReadOnlyList<ImportedBank>? Banks = null,
    IReadOnlyList<ImportedTestSet>? TestSets = null,
    IReadOnlyList<ImportedMedia>? Media = null)
{
    private static readonly JsonSerializerOptions Json = new(ItemJson.Options)
    {
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    /// <summary>Reads a file's JSON. Throws <see cref="JsonException"/> when it is malformed or misses a required field.</summary>
    public static QuestionImportFile Parse(string json) =>
        JsonSerializer.Deserialize<QuestionImportFile>(json, Json)
        ?? throw new JsonException("The file is empty.");
}

/// <summary>One question in an import file. Tags are given by code and slug, as people write them.</summary>
/// <param name="Id">Fixed id of the question; reuse it to update the question later.</param>
/// <param name="ItemType">snake_case, e.g. "multiple_choice" or "sata".</param>
/// <param name="Stem">The question text, Markdown.</param>
/// <param name="Content">The item type's content payload (interfaces.md section 2).</param>
/// <param name="Key">The item type's answer key.</param>
/// <param name="Rationale">Overall and per-option explanations; every option needs one.</param>
/// <param name="EducationalObjective">The one-sentence takeaway.</param>
/// <param name="ClientNeeds">Code of a Client Needs leaf for the file's track, e.g. "SECE.MOC".</param>
/// <param name="BodySystem">Body-system slug, e.g. "cardiovascular".</param>
/// <param name="Difficulty">easy, moderate or difficult.</param>
/// <param name="CjmmSkill">The CJMM step tested, if any.</param>
/// <param name="AccessTier">free (default) or premium.</param>
/// <param name="TimeAllotmentSec">Suggested time; 120 seconds by default, as in the DDL.</param>
/// <param name="Exhibits">A JSON array of exhibits; none by default.</param>
public sealed record ImportedQuestion(
    Guid Id,
    ItemType ItemType,
    string Stem,
    JsonElement Content,
    JsonElement Key,
    JsonElement Rationale,
    string EducationalObjective,
    string ClientNeeds,
    string BodySystem,
    DifficultyTier Difficulty,
    CjmmSkill? CjmmSkill = null,
    PlanTier AccessTier = PlanTier.Free,
    int TimeAllotmentSec = ImportedQuestion.DefaultTimeAllotmentSec,
    JsonElement? Exhibits = null)
{
    public const int DefaultTimeAllotmentSec = 120;
}

/// <summary>
/// A media asset in an import file: a file beside the import file, stored as a media_assets row
/// (database.md 2.4). Exhibits and options refer to it by <see cref="Id"/>.
/// </summary>
/// <param name="Id">Fixed id of the asset; exhibits use it as their mediaId.</param>
/// <param name="Kind">image, audio or video.</param>
/// <param name="File">Path of the file, relative to the import file's folder, e.g. "media/wound.png".</param>
/// <param name="MimeType">e.g. "image/png" or "audio/wav"; it must match the kind.</param>
/// <param name="AltText">Required for images.</param>
/// <param name="CaptionsFile">Path of a WebVTT file; required for audio and video.</param>
/// <param name="Width">Pixels, for images and video.</param>
/// <param name="Height">Pixels, for images and video.</param>
/// <param name="DurationMs">Length of an audio or video clip.</param>
public sealed record ImportedMedia(
    Guid Id,
    string Kind,
    string File,
    string MimeType,
    string? AltText = null,
    string? CaptionsFile = null,
    int? Width = null,
    int? Height = null,
    int? DurationMs = null);

/// <summary>A question bank in an import file. It is published when imported.</summary>
/// <param name="Id">Fixed id of the bank; reuse it to update the bank later.</param>
/// <param name="Name">Shown to candidates.</param>
/// <param name="QuestionIds">Ids of questions in this file, or already published on the same track.</param>
/// <param name="Description">Optional longer text.</param>
/// <param name="AccessTier">free (default) or premium.</param>
public sealed record ImportedBank(
    Guid Id,
    string Name,
    IReadOnlyList<Guid> QuestionIds,
    string? Description = null,
    PlanTier AccessTier = PlanTier.Free);

/// <summary>
/// A curated test set in an import file (PRD 5.5). It is imported as a published "prebuilt" set
/// whose items are served in the order of <see cref="QuestionIds"/>.
/// </summary>
/// <param name="Id">Fixed id of the set; reuse it to update the set later.</param>
/// <param name="Title">Shown on the set card.</param>
/// <param name="Mode">practice or challenge.</param>
/// <param name="RevealMode">tutor or exam.</param>
/// <param name="Timed">Whether a timer runs; a timed set needs perQuestionSec or timeBudgetSec.</param>
/// <param name="QuestionIds">Ids of questions in this file, or already published on the same track.</param>
/// <param name="Description">Optional text for the detail panel.</param>
/// <param name="PerQuestionSec">Seconds per item, for timed sets.</param>
/// <param name="TimeBudgetSec">Seconds for the whole attempt, for timed sets.</param>
/// <param name="PassThreshold">Share of points for a "likely pass" estimate, above 0 and at most 1.</param>
/// <param name="IsSample">Startable before verifying an email; a sample set has exactly 10 questions.</param>
/// <param name="AccessTier">free (default) or premium.</param>
/// <param name="AllowBackNav">Whether candidates may go back; true by default, as in the DDL.</param>
/// <param name="PausePolicy">Pause limits; no limits by default.</param>
public sealed record ImportedTestSet(
    Guid Id,
    string Title,
    TestMode Mode,
    RevealMode RevealMode,
    bool Timed,
    IReadOnlyList<Guid> QuestionIds,
    string? Description = null,
    int? PerQuestionSec = null,
    int? TimeBudgetSec = null,
    decimal? PassThreshold = null,
    bool IsSample = false,
    PlanTier AccessTier = PlanTier.Free,
    bool AllowBackNav = true,
    PausePolicy? PausePolicy = null)
{
    /// <summary>The set's settings as the domain stores them.</summary>
    public TestSetSettings ToSettings() => new(
        Title,
        Description,
        Mode,
        RevealMode,
        Timed,
        PerQuestionSec,
        TimeBudgetSec,
        PassThreshold,
        PausePolicy ?? PausePolicy.Unlimited,
        AllowBackNav,
        IsSample,
        AccessTier);
}
