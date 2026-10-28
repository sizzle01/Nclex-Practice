using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nclex.Application.Performance.GetBreakdown;
using Nclex.Application.Performance.GetSummary;
using Nclex.Application.Performance.GetTrend;

namespace Nclex.Api.Controllers;

/// <summary>
/// The signed-in user's performance across attempts (api.md section 5), for the dashboard and the
/// performance page. Summary and breakdown read the snapshots the worker keeps; they can lag a
/// submit by the few seconds the refresh job takes.
/// </summary>
[ApiController]
[Route("api/v1/me/performance")]
[Authorize]
[Produces("application/json")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
public sealed class PerformanceController : ControllerBase
{
    /// <summary>My totals in a track, and my weakest Client Needs categories.</summary>
    [HttpGet("summary")]
    [ProducesResponseType<PerformanceSummary>(StatusCodes.Status200OK)]
    public Task<PerformanceSummary> Summary(
        [FromQuery] GetSummaryRequest request,
        [FromServices] GetSummaryHandler handler,
        CancellationToken cancellationToken) =>
        handler.HandleAsync(request, cancellationToken);

    /// <summary>My accuracy, average time and item count per category of one dimension.</summary>
    [HttpGet("breakdown")]
    [ProducesResponseType<PerformanceBreakdown>(StatusCodes.Status200OK)]
    public Task<PerformanceBreakdown> Breakdown(
        [FromQuery] GetBreakdownRequest request,
        [FromServices] GetBreakdownHandler handler,
        CancellationToken cancellationToken) =>
        handler.HandleAsync(request, cancellationToken);

    /// <summary>My score over time, overall or in one category, by day or week in my time zone.</summary>
    [HttpGet("trend")]
    [ProducesResponseType<PerformanceTrend>(StatusCodes.Status200OK)]
    public Task<PerformanceTrend> Trend(
        [FromQuery] GetTrendRequest request,
        [FromServices] GetTrendHandler handler,
        CancellationToken cancellationToken) =>
        handler.HandleAsync(request, cancellationToken);
}
