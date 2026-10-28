using Nclex.Domain.Entities;

namespace Nclex.Domain.Grading;

/// <summary>
/// When a candidate may see an item's answer key, grade and rationale (architecture.md 5.5,
/// api.md GET /items). Nothing else decides this, so the rule cannot drift between endpoints.
/// </summary>
public static class RevealRules
{
    /// <summary>
    /// After the attempt is submitted, every item; before that, only items locked in tutor mode
    /// (submitted by the candidate, or locked when their time ran out).
    /// </summary>
    public static bool CanReveal(Attempt attempt, AttemptItem item) =>
        attempt.Status == AttemptStatus.Submitted
        || (attempt.Settings.RevealMode == RevealMode.Tutor && attempt.Status != AttemptStatus.Abandoned && item.IsLocked);
}
