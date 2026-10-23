using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nclex.Application.Abstractions;
using Nclex.Infrastructure.Persistence;
using StackExchange.Redis;

namespace Nclex.Infrastructure;

public static class DependencyInjection
{
    public const string PostgresConnectionName = "Postgres";
    public const string RedisConnectionName = "Redis";

    /// <summary>Registers the database context and the Redis connection used by both hosts.</summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Connection strings are read when the service is first resolved, not at registration,
        // so tests can swap configuration in after Program.cs has run.
        services.AddDbContext<AppDbContext>((provider, options) =>
            options
                .UseNpgsql(
                    GetRequiredConnectionString(provider, PostgresConnectionName),
                    // Keep EF's own bookkeeping table in the same snake_case style as everything else.
                    npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history"))
                .UseSnakeCaseNamingConvention());
        services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

        // Redis only holds data that can be rebuilt (architecture.md section 3), so the app must
        // start and keep serving when Redis is down. AbortOnConnectFail = false makes the
        // multiplexer retry in the background instead of throwing at startup.
        services.AddSingleton<IConnectionMultiplexer>(provider =>
        {
            var options = ConfigurationOptions.Parse(GetRequiredConnectionString(provider, RedisConnectionName));
            options.AbortOnConnectFail = false;
            return ConnectionMultiplexer.Connect(options);
        });

        return services;
    }

    private static string GetRequiredConnectionString(IServiceProvider provider, string name) =>
        provider.GetRequiredService<IConfiguration>().GetConnectionString(name)
        ?? throw new InvalidOperationException($"Connection string '{name}' is not configured.");
}
