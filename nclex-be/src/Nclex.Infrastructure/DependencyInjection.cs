using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nclex.Application.Abstractions;
using Nclex.Domain.Entities;
using Nclex.Infrastructure.Audit;
using Nclex.Infrastructure.Email;
using Nclex.Infrastructure.Persistence;
using Nclex.Infrastructure.RateLimiting;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.NameTranslation;
using StackExchange.Redis;

namespace Nclex.Infrastructure;

public static class DependencyInjection
{
    public const string PostgresConnectionName = "Postgres";
    public const string RedisConnectionName = "Redis";

    // One shared instance: EF Core compares options by value, and a new translator per DbContext
    // would make it build a new internal service provider for every request.
    private static readonly NpgsqlNullNameTranslator KeepNamesAsWritten = new();

    /// <summary>Registers the database context and the Redis connection used by both hosts.</summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Connection strings are read when the service is first resolved, not at registration,
        // so tests can swap configuration in after Program.cs has run.
        services.AddDbContext<AppDbContext>((provider, options) =>
            options
                .UseNpgsql(
                    GetRequiredConnectionString(provider, PostgresConnectionName),
                    npgsql => npgsql
                        // Keep EF's own bookkeeping table in the same snake_case style as everything else.
                        .MigrationsHistoryTable("__ef_migrations_history")
                        .MapDomainEnums())
                .UseSnakeCaseNamingConvention());
        services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());
        services.AddScoped<IEmailOutbox, EfEmailOutbox>();
        services.AddScoped<IAuditLog, EfAuditLog>();

        // Redis only holds data that can be rebuilt (architecture.md section 3), so the app must
        // start and keep serving when Redis is down. AbortOnConnectFail = false makes the
        // multiplexer retry in the background instead of throwing at startup.
        services.AddSingleton<IConnectionMultiplexer>(provider =>
        {
            var options = ConfigurationOptions.Parse(GetRequiredConnectionString(provider, RedisConnectionName));
            options.AbortOnConnectFail = false;
            return ConnectionMultiplexer.Connect(options);
        });
        services.AddSingleton<IRateLimitStore, RedisRateLimitStore>();

        return services;
    }

    /// <summary>
    /// Maps C# enums to the Postgres enums in database.md section 2.1. Values are snake_case
    /// ("candidate"), except exam_track, whose values are upper case ("RN", "PN") and are kept as written.
    /// </summary>
    private static NpgsqlDbContextOptionsBuilder MapDomainEnums(this NpgsqlDbContextOptionsBuilder npgsql) =>
        npgsql
            .MapEnum<ExamTrack>("exam_track", nameTranslator: KeepNamesAsWritten)
            .MapEnum<UserRole>("user_role")
            .MapEnum<PlanTier>("plan_tier")
            .MapEnum<EmailTokenPurpose>("email_token_purpose");

    private static string GetRequiredConnectionString(IServiceProvider provider, string name) =>
        provider.GetRequiredService<IConfiguration>().GetConnectionString(name)
        ?? throw new InvalidOperationException($"Connection string '{name}' is not configured.");
}
