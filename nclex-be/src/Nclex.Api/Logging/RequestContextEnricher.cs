using Nclex.Infrastructure.Security;
using Serilog.Core;
using Serilog.Events;

namespace Nclex.Api.Logging;

/// <summary>
/// Stamps every log event written during a request with its requestId, and with the userId and
/// attemptId when there are any (architecture.md section 9), so one candidate's problem can be
/// followed through the logs. Read when each event is written, so the request-completed line from
/// UseSerilogRequestLogging carries them too.
/// </summary>
/// <remarks>
/// ASP.NET Core's hosting log scope also adds a "RequestId" property, captured before any middleware
/// runs and holding the connection-based id, so this enricher overwrites it to keep a single id per
/// request across all log lines and the error body.
/// </remarks>
public sealed class RequestContextEnricher(IHttpContextAccessor httpContextAccessor) : ILogEventEnricher
{
    // Only AttemptsController routes name the attempt "id"; elsewhere "id" is a set, bank or flag.
    private const string AttemptsController = "Attempts";

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (httpContextAccessor.HttpContext is not { } httpContext)
        {
            return;
        }

        logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("RequestId", httpContext.TraceIdentifier));

        // The user id only: never the email or any other personal data.
        if (httpContext.User.FindFirst(AccessTokenClaims.Subject)?.Value is { } userId)
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("UserId", userId));
        }

        var route = httpContext.Request.RouteValues;
        if (Equals(route["controller"], AttemptsController) && route["id"] is { } attemptId)
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("AttemptId", attemptId));
        }
    }
}
