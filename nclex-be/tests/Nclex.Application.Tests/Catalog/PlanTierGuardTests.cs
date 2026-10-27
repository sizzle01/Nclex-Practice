using Nclex.Application.Catalog;
using Nclex.Application.Common;
using Nclex.Domain.Entities;

namespace Nclex.Application.Tests.Catalog;

public sealed class PlanTierGuardTests
{
    private static CatalogViewer Viewer(PlanTier plan) => new(Guid.NewGuid(), ExamTrack.RN, plan);

    [Fact]
    public void A_free_user_is_refused_premium_content_with_plan_upgrade_required()
    {
        var error = Should.Throw<AppException>(() => PlanTierGuard.EnsureAllowed(Viewer(PlanTier.Free), PlanTier.Premium));

        error.Error.ShouldBe(CatalogErrors.PlanUpgradeRequired);
        error.Error.Code.ShouldBe("PLAN_UPGRADE_REQUIRED");
        error.Error.Kind.ShouldBe(AppErrorKind.Forbidden);
    }

    [Theory]
    [InlineData(PlanTier.Free, PlanTier.Free)]
    [InlineData(PlanTier.Premium, PlanTier.Free)]
    [InlineData(PlanTier.Premium, PlanTier.Premium)]
    public void Allowed_content_passes(PlanTier plan, PlanTier contentTier) =>
        Should.NotThrow(() => PlanTierGuard.EnsureAllowed(Viewer(plan), contentTier));

    [Fact]
    public void Locked_is_the_opposite_of_allowed()
    {
        Viewer(PlanTier.Free).IsLocked(PlanTier.Premium).ShouldBeTrue();
        Viewer(PlanTier.Free).IsLocked(PlanTier.Free).ShouldBeFalse();
        Viewer(PlanTier.Premium).IsLocked(PlanTier.Premium).ShouldBeFalse();
    }
}
