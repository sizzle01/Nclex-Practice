namespace Nclex.Domain.Entities;

/// <summary>
/// The readiness estimate shown with a result (architecture.md 5.5). Always shown with a caveat:
/// it is an estimate, not a prediction of the real exam. An adaptive exam uses only
/// <see cref="LikelyReady"/> and <see cref="NeedsMorePractice"/>.
/// </summary>
public enum ReadinessLabel
{
    /// <summary>Pass: 86% and above (stored as <c>likely_ready</c>).</summary>
    LikelyReady,

    /// <summary>Close to pass: 70% to 85% (stored as <c>close_to_pass</c>).</summary>
    CloseToPass,

    /// <summary>Fail: below 70% (stored as <c>needs_more_practice</c>).</summary>
    NeedsMorePractice,
}
