using System.Text.Json;
using Nclex.Domain.Entities;
using Nclex.Domain.ItemTypes;

namespace Nclex.Application.Attempts.Reveal;

/// <summary>
/// The answer, grade and explanation of one item (api.md, GET /items <c>reveal</c>). Only built
/// when <see cref="Domain.Grading.RevealRules"/> allows it.
/// </summary>
/// <param name="AnswerKey">The item type's key shape, e.g. <c>{ "correct": ["a", "b"] }</c>.</param>
public sealed record ItemReveal(
    JsonElement AnswerKey,
    ItemGrade Grade,
    Rationale Rationale,
    string EducationalObjective,
    RevealTags Tags);

/// <summary>The grade of the final answer, with the per-target +1/0/-1 detail that shows partial credit.</summary>
public sealed record ItemGrade(decimal Points, decimal MaxPoints, bool IsCorrect, IReadOnlyList<GradeDetail> Detail);

/// <summary>The question's tags, by code and slug as in the taxonomy.</summary>
/// <param name="ClientNeeds">Client Needs code, e.g. "SECE.SIC".</param>
/// <param name="BodySystem">Body-system slug, e.g. "respiratory".</param>
public sealed record RevealTags(string ClientNeeds, string BodySystem, CjmmSkill? CjmmSkill, DifficultyTier Difficulty);
