using Nclex.Domain.Entities;

namespace Nclex.Domain.Grading;

/// <summary>The readiness estimate of a result; both parts are null when the set has no pass threshold.</summary>
public sealed record Readiness(bool? Passed, ReadinessLabel? Label)
{
    public static Readiness None { get; } = new(null, null);
}

/// <summary>Decides the readiness estimate from a score (interfaces.md section 3).</summary>
public interface IReadinessPolicy
{
    Readiness Evaluate(decimal scorePct, decimal? passThreshold);
}

/// <summary>
/// Reaching the set's threshold is "likely ready"; anything below is "needs more practice"
/// (architecture.md 5.5). Compared on the rounded score the candidate sees, so a shown 65.00%
/// against a 65% threshold is always a pass.
/// </summary>
public sealed class ReadinessPolicy : IReadinessPolicy
{
    public Readiness Evaluate(decimal scorePct, decimal? passThreshold)
    {
        if (passThreshold is not { } threshold)
        {
            return Readiness.None;
        }

        var passed = scorePct >= threshold;
        return new Readiness(passed, passed ? ReadinessLabel.LikelyReady : ReadinessLabel.NeedsMorePractice);
    }
}
