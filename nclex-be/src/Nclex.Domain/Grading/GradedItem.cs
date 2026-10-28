using Nclex.Domain.Entities;
using Nclex.Domain.ItemTypes;

namespace Nclex.Domain.Grading;

/// <summary>One graded item of an attempt, with what the results page needs to know about it.</summary>
/// <param name="IsAnswered">The candidate saved an answer; a blank item still counts towards the maximum.</param>
/// <param name="TimeSpentMs">Time on the item, as the server counted it.</param>
public sealed record GradedItem(
    decimal PointsEarned,
    decimal MaxPoints,
    bool IsCorrect,
    bool IsAnswered,
    long TimeSpentMs,
    ItemTags Tags);

/// <summary>A question's tags, as the keys stored in <c>attempt_category_scores</c>.</summary>
/// <param name="ClientNeedsCode">The leaf category, e.g. "SECE.MOC".</param>
/// <param name="ClientNeedsParentCode">Its top-level category, e.g. "SECE"; the leaf's own code when it is top-level.</param>
/// <param name="BodySystemSlug">E.g. "renal-urinary".</param>
public sealed record ItemTags(
    string ClientNeedsCode,
    string ClientNeedsParentCode,
    string BodySystemSlug,
    ItemType ItemType,
    CjmmSkill? CjmmSkill,
    DifficultyTier Difficulty);
