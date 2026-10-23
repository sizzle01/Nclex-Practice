using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Nclex.Api.Errors;
using Nclex.Api.Health;
using Nclex.Api.Logging;
using Nclex.Api.Middleware;
using Nclex.Infrastructure;
using Serilog;

// Bootstrap logger so failures during startup are still logged; replaced by the configured logger below.
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddSingleton<Serilog.Core.ILogEventEnricher, RequestIdEnricher>();
    builder.Services.AddSerilog((services, logger) => logger
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteToConsole(builder.Environment));

    // api.md: camelCase fields (web defaults) and snake_case enum strings, matching the database.
    // Set for both MVC (controller bodies) and minimal/HTTP JSON (used by the OpenAPI generator).
    var enumConverter = new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower);
    builder.Services.AddControllers()
        .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(enumConverter));
    builder.Services.ConfigureHttpJsonOptions(options => options.SerializerOptions.Converters.Add(enumConverter));

    builder.Services.AddOpenApi(options => options.AddDocumentTransformer((document, _, _) =>
    {
        document.Info.Title = "NCLEX Practice API";
        return Task.CompletedTask;
    }));
    builder.Services.AddApiProblemDetails();
    builder.Services.AddInfrastructure();
    builder.Services.AddReadinessChecks();
    builder.Services.AddSingleton(TimeProvider.System);

    var app = builder.Build();

    // Order matters: the request id must exist before anything logs, and the exception handler
    // must wrap everything that can throw.
    app.UseMiddleware<RequestIdMiddleware>();
    app.UseSerilogRequestLogging();
    app.UseExceptionHandler();
    app.UseApiStatusCodeProblemDetails();

    // ADR-8: the frontend's typed client is generated from this document.
    app.MapOpenApi("/api/v1/openapi.json");
    app.MapControllers();

    await app.RunAsync();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Nclex.Api terminated unexpectedly");
    throw;
}
finally
{
    await Log.CloseAndFlushAsync();
}

/// <summary>Exposed so integration tests can host the API with WebApplicationFactory.</summary>
public partial class Program;
