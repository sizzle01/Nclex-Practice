using Microsoft.Extensions.Diagnostics.HealthChecks;
using Nclex.Infrastructure.Persistence;

namespace Nclex.Api.Health;

/// <summary>Postgres is the system of record: without it the API cannot serve any request.</summary>
public sealed class PostgresHealthCheck(AppDbContext db) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await db.Database.CanConnectAsync(cancellationToken)
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy("Cannot connect to Postgres.");
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return HealthCheckResult.Unhealthy("Postgres check failed.", ex);
        }
    }
}
