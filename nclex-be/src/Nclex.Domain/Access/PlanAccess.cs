using Nclex.Domain.Entities;

namespace Nclex.Domain.Access;

/// <summary>
/// Which plans may use which content (architecture.md section 6, "Tier guard for premium sets and
/// banks"). Free content is open to everyone; premium content needs a premium plan.
/// </summary>
public static class PlanAccess
{
    /// <summary>True when an account on <paramref name="plan"/> may use content of <paramref name="contentTier"/>.</summary>
    public static bool Allows(PlanTier plan, PlanTier contentTier) =>
        contentTier == PlanTier.Free || plan == PlanTier.Premium;
}
