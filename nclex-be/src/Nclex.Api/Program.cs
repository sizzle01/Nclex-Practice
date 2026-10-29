using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Hangfire;
using Nclex.Api.Auth;
using Nclex.Api.Errors;
using Nclex.Api.Health;
using Nclex.Api.Logging;
using Nclex.Api.Middleware;
using Nclex.Api.ModelBinding;
using Nclex.Api.OpenApi;
using Nclex.Api.RateLimiting;
using Nclex.Api.Sse;
using Nclex.Application;
using Nclex.Infrastructure;
using Nclex.Infrastructure.Jobs;
using Nclex.Infrastructure.Security;
using Nclex.Infrastructure.Telemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Serilog;

// Bootstrap logger so failures during startup are still logged; replaced by the configured logger below.
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddSingleton<Serilog.Core.ILogEventEnricher, RequestContextEnricher>();
    builder.Services.AddSerilog((services, logger) => logger
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteToConsole(builder.Environment));

    // api.md: camelCase fields (web defaults) and snake_case enum strings, matching the database.
    // Set for both MVC (controller bodies) and minimal/HTTP JSON (used by the OpenAPI generator).
    var enumConverter = new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower);
    builder.Services.AddControllers(options =>
        {
            options.Filters.Add<FluentValidationFilter>();
            // Query-string enums use the same snake_case names as JSON (api.md section 1).
            options.ModelBinderProviders.Insert(0, new SnakeCaseEnumModelBinderProvider());
            // FluentValidation owns validation; without this, MVC would also reject missing
            // non-nullable strings itself, with a different error format.
            options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
        })
        .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(enumConverter))
        .ConfigureApiBehaviorOptions(options => options.InvalidModelStateResponseFactory = ValidationProblems.FromModelState);
    builder.Services.ConfigureHttpJsonOptions(options => options.SerializerOptions.Converters.Add(enumConverter));

    builder.Services.AddOpenApi(options =>
    {
        options.AddDocumentTransformer((document, _, _) =>
        {
            document.Info.Title = "NCLEX Practice API";
            return Task.CompletedTask;
        });
        options.AddOperationTransformer<CamelCaseQueryParameters>();
    });
    builder.Services.AddApiProblemDetails();
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure();
    builder.Services.AddNotificationStreamTickets();
    // Client only: the API schedules expiry jobs and Nclex.Worker runs them.
    builder.Services.AddJobScheduling();
    builder.Services.AddSecurity();
    builder.Services.AddMediaLinks();
    builder.Services.AddApiAuth();
    builder.Services.AddApiRateLimiting();
    builder.Services.AddReadinessChecks();
    builder.Services.AddTrustedProxies(builder.Configuration);
    builder.Services.AddSingleton(TimeProvider.System);
    builder.Services.AddNclexTelemetry(builder.Configuration, "nclex-api")
        .WithTracing(tracing => tracing.AddAspNetCoreInstrumentation(options =>
            // Probes arrive every few seconds and would bury real traffic.
            options.Filter = http => !http.Request.Path.StartsWithSegments("/healthz")
                && !http.Request.Path.StartsWithSegments("/readyz")))
        // http.server.request.duration: the p95 latency the alerts and load test use.
        .WithMetrics(metrics => metrics.AddAspNetCoreInstrumentation());

    var app = builder.Build();
    // Fail fast if an MVP item type has no grader (interfaces.md section 3).
    app.Services.EnsureItemTypeModules();

    // Order matters: the real client address and scheme come first, so rate limits and HSTS see
    // them; the request id must exist before anything logs; the exception handler must wrap
    // everything that can throw.
    app.UseForwardedHeaders();
    app.UseMiddleware<SecurityHeadersMiddleware>();
    if (!app.Environment.IsDevelopment())
    {
        // TLS terminates at the load balancer; X-Forwarded-Proto marks the request as HTTPS.
        app.UseHsts();
    }

    app.UseMiddleware<RequestIdMiddleware>();
    app.UseSerilogRequestLogging();
    app.UseExceptionHandler();
    app.UseApiStatusCodeProblemDetails();
    // After routing (implicit in minimal hosting), so it can see each endpoint's policy.
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();

    // ADR-8: the frontend's typed client is generated from this document.
    app.MapOpenApi("/api/v1/openapi.json").AllowAnonymous();
    if (app.Environment.IsDevelopment())
    {
        // Hangfire's own page of scheduled and recurring jobs; it only answers requests from this machine.
        app.UseHangfireDashboard("/hangfire");
    }

    // Secure by default: every controller action needs a signed-in user unless marked [AllowAnonymous].
    // (A fallback policy would also turn 404s for unknown routes into 401s.)
    app.MapControllers().RequireAuthorization();
    // Authenticated by its own single-use ticket, since EventSource cannot send a bearer token.
    app.MapNotificationStream();

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
