using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nclex.Application.Abstractions;
using Nclex.Domain.Entities;
using Nclex.Domain.ItemTypes;
using Nclex.Domain.Notifications;
using Nclex.Infrastructure.Audit;
using Nclex.Infrastructure.Email;
using Nclex.Infrastructure.Notifications;
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
                // Pushes new notifications to Redis once their transaction commits (architecture.md 5.5).
                .AddInterceptors(provider.GetRequiredService<PublishNotificationsAfterCommit>())
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

        services.AddScoped<INotificationPublisher, NotificationPublisher>();
        services.AddScoped<PendingNotifications>();
        services.AddScoped<PublishNotificationsAfterCommit>();

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
        services.AddSingleton<INotificationBus, RedisNotificationBus>();
        services.AddScoped<IStreamTicketStore, RedisStreamTicketStore>();

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
            .MapEnum<EmailTokenPurpose>("email_token_purpose")
            .MapEnum<ItemType>("item_type")
            .MapEnum<DifficultyTier>("difficulty_tier")
            .MapEnum<CjmmSkill>("cjmm_skill")
            .MapEnum<VersionStatus>("version_status")
            .MapEnum<QuestionStatus>("question_status")
            .MapEnum<TestSetKind>("test_set_kind")
            .MapEnum<TestMode>("test_mode")
            .MapEnum<RevealMode>("reveal_mode")
            .MapEnum<AttemptStatus>("attempt_status")
            .MapEnum<SubmitReason>("submit_reason")
            .MapEnum<ItemState>("item_state")
            .MapEnum<NotificationType>("notification_type")
            .MapEnum<NotificationCategory>("notification_category");

    private static string GetRequiredConnectionString(IServiceProvider provider, string name) =>
        provider.GetRequiredService<IConfiguration>().GetConnectionString(name)
        ?? throw new InvalidOperationException($"Connection string '{name}' is not configured.");
}
