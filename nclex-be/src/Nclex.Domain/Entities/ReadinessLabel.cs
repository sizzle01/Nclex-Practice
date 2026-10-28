namespace Nclex.Domain.Entities;

/// <summary>
/// The readiness estimate shown with a result when the set has a pass threshold (architecture.md
/// 5.5). Always shown with a caveat: it is an estimate, not a prediction of the real exam.
/// </summary>
public enum ReadinessLabel
{
    LikelyReady,
    NeedsMorePractice,
}
