using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Nclex.Api.Health;

namespace Nclex.Api.Controllers;

/// <summary>
/// Liveness and readiness probes (api.md section 9). They sit at the root, outside /api/v1,
/// because load balancers and container orchestrators probe fixed paths.
/// </summary>
[ApiController]
[AllowAnonymous]
[Produces("application/json")]
public sealed class HealthController(HealthCheckService healthChecks) : ControllerBase
{
    /// <summary>Liveness: the process is up. Never touches dependencies, so a database outage does not restart the API.</summary>
    [HttpGet("/healthz")]
    [ProducesResponseType<LivenessResponse>(StatusCodes.Status200OK)]
    public LivenessResponse Liveness() => new(HealthState.Ok);

    /// <summary>Readiness: can the API serve traffic? 503 only when Postgres is unreachable; Redis down is "degraded".</summary>
    [HttpGet("/readyz")]
    [ProducesResponseType<ReadinessResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ReadinessResponse>(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<ReadinessResponse>> Readiness(CancellationToken cancellationToken)
    {
        var report = await healthChecks.CheckHealthAsync(cancellationToken);
        var response = new ReadinessResponse(
            ToState(report.Status),
            new ReadinessChecks(
                Postgres: ToState(report.Entries[HealthSetup.PostgresCheck].Status),
                Redis: ToState(report.Entries[HealthSetup.RedisCheck].Status)));

        return report.Status == HealthStatus.Unhealthy
            ? StatusCode(StatusCodes.Status503ServiceUnavailable, response)
            : Ok(response);
    }

    private static HealthState ToState(HealthStatus status) => status switch
    {
        HealthStatus.Healthy => HealthState.Ok,
        HealthStatus.Degraded => HealthState.Degraded,
        _ => HealthState.Unhealthy,
    };
}
