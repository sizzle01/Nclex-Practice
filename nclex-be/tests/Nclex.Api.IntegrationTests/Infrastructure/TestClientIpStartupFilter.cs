using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace Nclex.Api.IntegrationTests.Infrastructure;

/// <summary>
/// TestServer requests have no client IP address. This sets it from a test-only header, so tests
/// can act as different clients for the per-IP limits. Runs before the API's own middleware.
/// </summary>
public sealed class TestClientIpStartupFilter : IStartupFilter
{
    public const string Header = "X-Test-Client-Ip";

    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
    {
        app.Use((context, nextMiddleware) =>
        {
            if (IPAddress.TryParse(context.Request.Headers[Header], out var ip))
            {
                context.Connection.RemoteIpAddress = ip;
            }

            return nextMiddleware(context);
        });
        next(app);
    };

    /// <summary>A random address in the 10.0.0.0/8 private range, so tests never share one.</summary>
    public static string NewIp() => $"10.{Random.Shared.Next(256)}.{Random.Shared.Next(256)}.{Random.Shared.Next(1, 255)}";
}
