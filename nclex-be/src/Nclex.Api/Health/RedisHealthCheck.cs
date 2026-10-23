using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace Nclex.Api.Health;

/// <summary>
/// Redis only holds rebuildable data (rate limits, SSE fan-out, cache), so losing it degrades the
/// API rather than taking it down (api.md section 9: "Redis as degraded-OK"). The registration sets
/// the failure status to Degraded.
/// </summary>
public sealed class RedisHealthCheck(IConnectionMultiplexer redis) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        if (!redis.IsConnected)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, "Not connected to Redis.");
        }

        try
        {
            await redis.GetDatabase().PingAsync();
            return HealthCheckResult.Healthy();
        }
        catch (RedisException ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, "Redis ping failed.", ex);
        }
    }
}
