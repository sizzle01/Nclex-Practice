using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Nclex.Api.Logging;
using Serilog.Core;
using Serilog.Events;
using Serilog.Parsing;

namespace Nclex.Api.IntegrationTests.Observability;

/// <summary>Every request log line carries requestId, and userId and attemptId when known (architecture.md section 9).</summary>
public sealed class RequestContextEnricherTests
{
    [Fact]
    public void A_signed_in_request_on_an_attempt_route_gets_all_three_ids()
    {
        var attemptId = Guid.NewGuid().ToString();
        var http = Request("Attempts", attemptId, userId: "user-1");

        var logEvent = Enrich(http);

        Property(logEvent, "RequestId").ShouldBe("req_test");
        Property(logEvent, "UserId").ShouldBe("user-1");
        Property(logEvent, "AttemptId").ShouldBe(attemptId);
    }

    [Fact]
    public void An_id_on_another_controller_is_not_taken_for_an_attempt()
    {
        var http = Request("TestSets", Guid.NewGuid().ToString(), userId: null);

        var logEvent = Enrich(http);

        logEvent.Properties.ShouldContainKey("RequestId");
        logEvent.Properties.ShouldNotContainKey("UserId");
        logEvent.Properties.ShouldNotContainKey("AttemptId");
    }

    private static DefaultHttpContext Request(string controller, string id, string? userId)
    {
        var http = new DefaultHttpContext { TraceIdentifier = "req_test" };
        http.Request.RouteValues["controller"] = controller;
        http.Request.RouteValues["id"] = id;
        if (userId is not null)
        {
            http.User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", userId)], "test"));
        }

        return http;
    }

    private static LogEvent Enrich(HttpContext http)
    {
        var logEvent = new LogEvent(DateTimeOffset.UtcNow, LogEventLevel.Information, null, new MessageTemplate("test", []), []);
        new RequestContextEnricher(new HttpContextAccessor { HttpContext = http }).Enrich(logEvent, new PropertyFactory());
        return logEvent;
    }

    private static object? Property(LogEvent logEvent, string name) =>
        ((ScalarValue)logEvent.Properties[name]).Value;

    private sealed class PropertyFactory : ILogEventPropertyFactory
    {
        public LogEventProperty CreateProperty(string name, object? value, bool destructureObjects = false) =>
            new(name, new ScalarValue(value));
    }
}
