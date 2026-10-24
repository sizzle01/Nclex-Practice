namespace Nclex.Domain.Entities;

/// <summary>Subscription level that gates premium sets and banks (database enum <c>plan_tier</c>).</summary>
public enum PlanTier
{
    /// <summary>Default for every new account.</summary>
    Free,

    /// <summary>Unlocks premium content. No payment integration in the MVP (docs/README.md assumption 3).</summary>
    Premium,
}
