using Nclex.Domain.Access;
using Nclex.Domain.Entities;

namespace Nclex.Domain.Tests.Access;

public sealed class PlanAccessTests
{
    [Theory]
    [InlineData(PlanTier.Free, PlanTier.Free, true)]
    [InlineData(PlanTier.Premium, PlanTier.Free, true)]
    [InlineData(PlanTier.Premium, PlanTier.Premium, true)]
    [InlineData(PlanTier.Free, PlanTier.Premium, false)]
    public void Only_premium_content_on_a_free_plan_is_refused(PlanTier plan, PlanTier contentTier, bool allowed) =>
        PlanAccess.Allows(plan, contentTier).ShouldBe(allowed);
}
