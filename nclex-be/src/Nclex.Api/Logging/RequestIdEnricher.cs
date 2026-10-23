using Serilog.Core;
using Serilog.Events;

namespace Nclex.Api.Logging;

/// <summary>
/// Stamps every log event written during a request with our requestId (set by RequestIdMiddleware).
/// ASP.NET Core's hosting log scope also adds a "RequestId" property, captured before any middleware
/// runs and holding the connection-based id, so this enricher overwrites it to keep a single id per
/// request across all log lines and the error body.
/// </summary>
public sealed class RequestIdEnricher(IHttpContextAccessor httpContextAccessor) : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (httpContextAccessor.HttpContext is { } httpContext)
        {
            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("RequestId", httpContext.TraceIdentifier));
        }
    }
}
