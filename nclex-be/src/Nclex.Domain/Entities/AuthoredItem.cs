using System.Text.Json;
using Nclex.Domain.ItemTypes;

namespace Nclex.Domain.Entities;

/// <summary>
/// What an author writes for one question version: the item itself and its tags. The payloads are
/// raw JSON whose shape depends on <see cref="ItemType"/> (interfaces.md section 2).
/// </summary>
/// <param name="ItemType">Which item-type module validates and grades it.</param>
/// <param name="Stem">The question text, sanitised Markdown.</param>
/// <param name="Content">Render payload, safe to send to candidates.</param>
/// <param name="AnswerKey">Never sent to candidates before the reveal.</param>
/// <param name="Rationale">Per-option and overall explanation, hidden until the reveal.</param>
/// <param name="EducationalObjective">The one-sentence takeaway shown with the rationale.</param>
/// <param name="Exhibits">Charts, flowsheets and media shown beside the stem; a JSON array.</param>
/// <param name="ClientNeedsId">A leaf of the Client Needs tree.</param>
/// <param name="BodySystemId">The body system tag.</param>
/// <param name="CjmmSkill">The clinical judgment step it tests, if any.</param>
/// <param name="Difficulty">The author's difficulty rating.</param>
/// <param name="TimeAllotmentSec">Suggested time on the item, in seconds.</param>
/// <param name="MaxPoints">From the item-type module, worked out from the key.</param>
public sealed record AuthoredItem(
    ItemType ItemType,
    string Stem,
    JsonElement Content,
    JsonElement AnswerKey,
    JsonElement Rationale,
    string EducationalObjective,
    JsonElement Exhibits,
    short ClientNeedsId,
    short BodySystemId,
    CjmmSkill? CjmmSkill,
    DifficultyTier Difficulty,
    int TimeAllotmentSec,
    short MaxPoints);
