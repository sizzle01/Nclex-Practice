namespace Nclex.Application.Catalog.CustomSets;

/// <summary>GET /test-sets/custom/available-count: the live pool size the builder shows while filters change.</summary>
public sealed class GetAvailableCountHandler(PlanTierGuard planTierGuard, QuestionPool questionPool)
{
    public async Task<AvailableCountResponse> HandleAsync(AvailableCountRequest request, CancellationToken cancellationToken)
    {
        var viewer = await planTierGuard.LoadViewerAsync(cancellationToken);
        var pool = await questionPool.LoadAsync(request, viewer, cancellationToken);
        return new AvailableCountResponse(pool.Items.Count);
    }
}
