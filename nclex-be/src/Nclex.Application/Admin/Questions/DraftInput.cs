using System.Text.Json;
using Nclex.Domain.Entities;
using Nclex.Domain.ItemTypes;

namespace Nclex.Application.Admin.Questions;

/// <summary>
/// What an author writes for a draft (PUT /admin/question-versions/{id}, and the draft of POST
/// /admin/questions). The payloads are raw JSON in the item type's shape (interfaces.md section 2);
/// <see cref="AuthoringValidator"/> checks them all on every save (database.md 2.4 rules).
/// </summary>
/// <param name="ItemType">An MVP item type.</param>
/// <param name="Stem">The question text, Markdown.</param>
/// <param name="Content">What candidates see: options, grid, blanks...</param>
/// <param name="AnswerKey">The correct answer. Never sent to candidates before the reveal.</param>
/// <param name="Rationale">{ correct, perOption, references? }; perOption covers every selectable id.</param>
/// <param name="EducationalObjective">The one-sentence takeaway shown with the rationale.</param>
/// <param name="Exhibits">Charts, tables, text and media beside the stem; an empty array when there are none.</param>
/// <param name="ClientNeedsId">A leaf of the track's Client Needs tree (GET /taxonomy ids).</param>
/// <param name="BodySystemId">A body system id from GET /taxonomy.</param>
/// <param name="CjmmSkill">The clinical judgment step it tests, if any.</param>
/// <param name="Difficulty">The author's rating.</param>
/// <param name="TimeAllotmentSec">Suggested seconds on the item; 120 when left out (the column default).</param>
public sealed record DraftInput(
    ItemType ItemType,
    string Stem,
    JsonElement Content,
    JsonElement AnswerKey,
    JsonElement Rationale,
    string EducationalObjective,
    JsonElement? Exhibits,
    short ClientNeedsId,
    short BodySystemId,
    CjmmSkill? CjmmSkill,
    DifficultyTier Difficulty,
    int? TimeAllotmentSec)
{
    public const int DefaultTimeAllotmentSec = 120;
}
