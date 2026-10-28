using System.Globalization;
using Nclex.Application;
using Nclex.Infrastructure;
using Nclex.Infrastructure.Email;
using Nclex.Infrastructure.Jobs;
using Nclex.Worker;
using Serilog;
using Serilog.Formatting.Compact;

// "import-questions <file>" runs the content importer once and exits instead of starting the worker.
if (args is [ImportQuestionsCommand.Name, .. var commandArgs])
{
    return await ImportQuestionsCommand.RunAsync(commandArgs);
}

// "set-role <email> <role>" gives an account a role, e.g. the first admin, and exits.
if (args is [SetRoleCommand.Name, .. var roleArgs])
{
    return await SetRoleCommand.RunAsync(roleArgs);
}

// Nclex.Worker hosts background work (architecture.md section 3: expiry, email outbox,
// aggregation, reminders): the email outbox dispatcher (F02) and the Hangfire server with the
// attempt expiry jobs (F09).
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
    .CreateBootstrapLogger();

try
{
    var builder = Host.CreateApplicationBuilder(args);

    builder.Services.AddSerilog((services, logger) =>
    {
        logger
            .ReadFrom.Configuration(builder.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext();

        // Same convention as the API: readable text locally, JSON elsewhere.
        if (builder.Environment.IsDevelopment())
        {
            logger.WriteTo.Console(formatProvider: CultureInfo.InvariantCulture);
        }
        else
        {
            logger.WriteTo.Console(new RenderedCompactJsonFormatter());
        }
    });

    builder.Services.AddInfrastructure();
    builder.Services.AddEmailDelivery();
    builder.Services.AddHostedService<EmailOutboxDispatcherService>();
    builder.Services.AddAttemptExpiry();
    builder.Services.AddJobScheduling();
    builder.Services.AddJobServer();
    builder.Services.AddSingleton(TimeProvider.System);

    var host = builder.Build();
    // The expiry jobs grade attempts, so a missing item-type module must stop the worker at start.
    host.Services.EnsureItemTypeModules();
    host.Services.ScheduleRecurringJobs();
    host.Services.GetRequiredService<IHostApplicationLifetime>().ApplicationStarted.Register(() =>
        Log.Information("Nclex.Worker started in {Environment}", builder.Environment.EnvironmentName));

    await host.RunAsync();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Nclex.Worker terminated unexpectedly");
    throw;
}
finally
{
    await Log.CloseAndFlushAsync();
}

return 0;
