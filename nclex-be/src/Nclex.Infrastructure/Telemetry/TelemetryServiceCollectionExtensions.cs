using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nclex.Application.Common;
using Npgsql;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Nclex.Infrastructure.Telemetry;

public static class TelemetryServiceCollectionExtensions
{
    /// <summary>
    /// The standard OpenTelemetry variable naming the collector. When it is not set (tests, a quick
    /// local run) nothing is exported, instead of the exporter retrying a collector that isn't there.
    /// </summary>
    public const string OtlpEndpointSetting = "OTEL_EXPORTER_OTLP_ENDPOINT";

    /// <summary>
    /// Traces and metrics shared by both hosts (architecture.md section 9): spans for outgoing HTTP,
    /// Postgres and Redis calls, and the app's own <see cref="NclexMetrics"/>. The API adds its
    /// ASP.NET Core instrumentation on the builder this returns.
    /// </summary>
    /// <param name="serviceName">Shown as service.name, e.g. "nclex-api", so traces say which host they came from.</param>
    public static OpenTelemetryBuilder AddNclexTelemetry(
        this IServiceCollection services,
        IConfiguration configuration,
        string serviceName)
    {
        var builder = services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(serviceName))
            .WithTracing(tracing => tracing
                .AddHttpClientInstrumentation()
                .AddNpgsql()
                // Finds the IConnectionMultiplexer registered by AddInfrastructure.
                .AddRedisInstrumentation())
            .WithMetrics(metrics => metrics
                .AddHttpClientInstrumentation()
                .AddNpgsqlInstrumentation()
                .AddMeter(NclexMetrics.MeterName));

        if (!string.IsNullOrWhiteSpace(configuration[OtlpEndpointSetting]))
        {
            // Reads the endpoint, protocol and headers from the standard OTEL_EXPORTER_OTLP_* variables.
            builder.UseOtlpExporter();
        }

        return builder;
    }
}
