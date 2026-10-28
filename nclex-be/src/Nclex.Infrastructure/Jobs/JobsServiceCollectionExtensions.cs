using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nclex.Application.Abstractions;

namespace Nclex.Infrastructure.Jobs;

public static class JobsServiceCollectionExtensions
{
    /// <summary>Hangfire keeps its tables in their own schema, away from the EF Core migrations.</summary>
    public const string HangfireSchema = "hangfire";

    /// <summary>
    /// Hangfire as a client: lets the API schedule jobs in Postgres (architecture.md section 4). Both
    /// hosts call it; only the worker also adds the server that runs them (<see cref="AddJobServer"/>).
    /// </summary>
    public static IServiceCollection AddJobScheduling(this IServiceCollection services)
    {
        // The connection string is read when Hangfire is first used, so tests can swap it in late.
        services.AddHangfire((provider, config) => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(
                options => options.UseNpgsqlConnection(
                    provider.GetRequiredService<IConfiguration>().GetConnectionString(DependencyInjection.PostgresConnectionName)),
                new PostgreSqlStorageOptions { SchemaName = HangfireSchema }));
        services.AddScoped<IJobScheduler, HangfireJobScheduler>();
        return services;
    }

    /// <summary>The Hangfire server and the job classes it runs; for Nclex.Worker only.</summary>
    public static IServiceCollection AddJobServer(this IServiceCollection services)
    {
        services.AddHangfireServer();
        services.AddScoped<AttemptExpiryJob>();
        services.AddScoped<ExpirySweeperJob>();
        services.AddScoped<DraftReminderJob>();
        return services;
    }

    /// <summary>
    /// Registers the recurring jobs: the expiry sweeper every minute, Hangfire's shortest interval
    /// (architecture.md 5.1, rule 3), and the draft reminders on <c>Notifications:DraftReminderCron</c>.
    /// </summary>
    public static void ScheduleRecurringJobs(this IServiceProvider services)
    {
        var jobs = services.GetRequiredService<IRecurringJobManager>();
        jobs.AddOrUpdate<ExpirySweeperJob>(ExpirySweeperJob.RecurringJobId, job => job.RunAsync(CancellationToken.None), Cron.Minutely());
        jobs.AddOrUpdate<DraftReminderJob>(
            DraftReminderJob.RecurringJobId,
            job => job.RunAsync(CancellationToken.None),
            NotificationJobSettings.ReminderCron(services.GetRequiredService<IConfiguration>()));
    }
}
