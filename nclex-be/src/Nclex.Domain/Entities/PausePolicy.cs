namespace Nclex.Domain.Entities;

/// <summary>
/// How often and how long an attempt of a set may be paused. Stored as the <c>pause_policy</c> JSON
/// column of <c>test_sets</c>; null means no limit. Enforced by the attempt engine (F08).
/// </summary>
/// <param name="MaxPauses">How many pauses an attempt may use.</param>
/// <param name="MaxPauseSec">How long one pause may last, in seconds.</param>
public sealed record PausePolicy(int? MaxPauses, int? MaxPauseSec)
{
    /// <summary>The DDL default: pause as often and as long as you like.</summary>
    public static PausePolicy Unlimited => new(null, null);
}
