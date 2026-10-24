using Microsoft.Net.Http.Headers;
using Nclex.Domain.Entities;

namespace Nclex.Api.Auth;

public static class HttpContextClientInfo
{
    // User-Agent strings are client-controlled; cap what is stored.
    private const int MaxUserAgentLength = 512;

    /// <summary>The browser and IP address of the current request, stored with each session.</summary>
    public static ClientInfo GetClientInfo(this HttpContext context)
    {
        string? userAgent = context.Request.Headers[HeaderNames.UserAgent];
        if (userAgent?.Length > MaxUserAgentLength)
        {
            userAgent = userAgent[..MaxUserAgentLength];
        }

        return new ClientInfo(userAgent, context.Connection.RemoteIpAddress);
    }
}
