using System.Globalization;
using Nclex.Infrastructure;
using Nclex.Infrastructure.Email;
using Nclex.Worker;
using Serilog;
using Serilog.Formatting.Compact;

// "import-questions <file>" runs the content importer once and exits instead of starting the worker.
if (args is [ImportQuestionsCommand.Name, .. var commandArgs])
{
    return await ImportQuestionsCommand.RunAsync(commandArgs);
}

// Nclex.Worker hosts background work (architecture.md section 3: expiry, email outbox,
// aggregation, reminders). F02 adds the email outbox dispatcher; Hangfire and jobs arrive with later features.
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
    builder.Services.AddSingleton(TimeProvider.System);

    var host = builder.Build();
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
