using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nclex.Api.Auth;
using Nclex.Application.Performance.RebuildSnapshots;

namespace Nclex.Api.Controllers;

/// <summary>Repairs for the derived performance snapshots (api.md section 8, admin only).</summary>
[ApiController]
[Route("api/v1/admin/performance-snapshots")]
[Authorize(Policy = Policies.Admin)]
[Produces("application/json")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
public sealed class AdminPerformanceSnapshotsController : ControllerBase
{
    /// <summary>Queue a rebuild of one user's snapshots from their results. The worker does it shortly after.</summary>
    [HttpPost("rebuild")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<IActionResult> Rebuild(
        RebuildSnapshotsRequest request,
        [FromServices] RequestSnapshotRebuildHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(request, cancellationToken);
        return Accepted();
    }
}
