using Nclex.Domain.Entities;

namespace Nclex.Domain.Grading;

/// <summary>The readiness estimate of a result: whether it passed, and which of the three outcomes it is.</summary>
public sealed record Readiness(bool Passed, ReadinessLabel Label);

/// <summary>Decides the readiness estimate from a score (interfaces.md section 3).</summary>
public interface IReadinessPolicy
{
    Readiness Evaluate(decimal scorePct);
}

/// <summary>
/// One fixed rule for every fixed-length test (architecture.md 5.5, F23): 86% and above is a
/// pass, 70% to 85% is close to pass, below 70% is a fail. The set's own threshold no longer
/// counts. Compared on the whole percentage the candidate sees, so a shown "86%" (85.5% or more)
/// always passes and a shown "70%" (69.5% or more) is always close to pass.
/// </summary>
public sealed class ReadinessPolicy : IReadinessPolicy
{
    /// <summary>The lowest shown percentage that passes.</summary>
    public const int PassMarkPercent = 86;

    /// <summary>The lowest shown percentage that is close to pass; anything below fails.</summary>
    public const int CloseToPassPercent = 70;

    /// <summary>The pass mark as a share, stored on each result as its threshold.</summary>
    public const decimal PassMark = PassMarkPercent / 100m;

    public Readiness Evaluate(decimal scorePct)
    {
        var shown = ScoreMaths.WholePercent(scorePct);
        if (shown >= PassMarkPercent)
        {
            return new Readiness(true, ReadinessLabel.LikelyReady);
        }

        return new Readiness(false, shown >= CloseToPassPercent ? ReadinessLabel.CloseToPass : ReadinessLabel.NeedsMorePractice);
    }
}
