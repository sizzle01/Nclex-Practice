using Microsoft.AspNetCore.HttpOverrides;

namespace Nclex.Api.Middleware;

public static class ForwardedHeadersSetup
{
    /// <summary>Configuration key: CIDR ranges of the load balancer and the Next.js server, e.g. ["10.0.0.0/16"].</summary>
    public const string KnownNetworksSetting = "ForwardedHeaders:KnownNetworks";

    /// <summary>
    /// Trusts X-Forwarded-For and X-Forwarded-Proto only from our own proxies (architecture.md
    /// section 8: browser → load balancer → Next.js → API). Without it every request would carry the
    /// proxy's address, and the per-IP login limit and stored session IPs would be shared by everyone.
    /// Loopback is always trusted, which covers local runs where Next.js forwards on localhost.
    /// </summary>
    public static IServiceCollection AddTrustedProxies(this IServiceCollection services, IConfiguration configuration)
    {
        var networks = configuration.GetSection(KnownNetworksSetting).Get<string[]>() ?? [];
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            // Walk back through every trusted proxy to the first address we did not add ourselves.
            options.ForwardLimit = null;
            foreach (var network in networks)
            {
                options.KnownIPNetworks.Add(System.Net.IPNetwork.Parse(network));
            }
        });
        return services;
    }
}
