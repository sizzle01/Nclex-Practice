using System.Text.Json;
using Nclex.Domain.Entities;
using Nclex.Domain.ItemTypes;

namespace Nclex.Application.Content.ImportQuestions;

/// <summary>
/// A JSON file of questions for the content importer. Every question carries a fixed id, so importing
/// the same file again updates those questions instead of adding copies.
/// </summary>
/// <param name="Track">The exam track of every question in the file.</param>
/// <param name="Questions">The questions to publish.</param>
/// <param name="Notice">Free text for people reading the file, e.g. that it is sample content. Not stored.</param>
public sealed record QuestionImportFile(ExamTrack Track, IReadOnlyList<ImportedQuestion> Questions, string? Notice = null)
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
