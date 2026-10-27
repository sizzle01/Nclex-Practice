using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Nclex.Api.Auth;
using Nclex.Api.Errors;
using Nclex.Api.Health;
using Nclex.Api.Logging;
using Nclex.Api.Middleware;
using Nclex.Api.ModelBinding;
using Nclex.Api.OpenApi;
using Nclex.Api.RateLimiting;
using Nclex.Application;
using Nclex.Infrastructure;
using Nclex.Infrastructure.Security;
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
    builder.Services.AddSecurity();
    builder.Services.AddApiAuth();
    builder.Services.AddApiRateLimiting();
    builder.Services.AddReadinessChecks();
    builder.Services.AddSingleton(TimeProvider.System);

    var app = builder.Build();
    // Fail fast if an MVP item type has no grader (interfaces.md section 3).
    app.Services.EnsureItemTypeModules();

    // Order matters: the request id must exist before anything logs, and the exception handler
    // must wrap everything that can throw.
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
    // Secure by default: every controller action needs a signed-in user unless marked [AllowAnonymous].
    // (A fallback policy would also turn 404s for unknown routes into 401s.)
    app.MapControllers().RequireAuthorization();

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
