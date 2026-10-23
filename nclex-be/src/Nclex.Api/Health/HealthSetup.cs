using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Nclex.Api.Health;

public static class HealthSetup
{
    public const string PostgresCheck = "postgres";
    public const string RedisCheck = "redis";

    public static IServiceCollection AddReadinessChecks(this IServiceCollection services)
    {
        var checkTimeout = TimeSpan.FromSeconds(3);
        services.AddHealthChecks()
            .AddCheck<PostgresHealthCheck>(PostgresCheck, HealthStatus.Unhealthy, timeout: checkTimeout)
            .AddCheck<RedisHealthCheck>(RedisCheck, HealthStatus.Degraded, timeout: checkTimeout);
        return services;
    }
}
