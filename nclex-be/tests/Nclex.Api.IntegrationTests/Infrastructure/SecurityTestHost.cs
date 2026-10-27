using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Nclex.Api.IntegrationTests.Infrastructure;

/// <summary>
/// A copy of the shared API host with real Redis and a manual clock, for rate-limit and lockout
/// tests. It uses the same Postgres database. Each test gets its own host, clock and client IP,
/// so tests never see each other's counters.
/// </summary>
public sealed class SecurityTestHost : IAsyncDisposable
{
    private readonly WebApplicationFactory<Program> _host;

    public SecurityTestHost(ApiFactory factory)
    {
        // Starts at the real time, so access tokens issued now still pass JWT validation.
        Clock = new ManualTimeProvider(DateTimeOffset.UtcNow);
        _host = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:Redis", factory.RedisConnectionString);
            builder.ConfigureTestServices(services => services.Replace(ServiceDescriptor.Singleton<TimeProvider>(Clock)));
        });
        ClientIp = TestClientIpStartupFilter.NewIp();
        Api = new AuthApi(_host, ClientIp);
    }

    public ManualTimeProvider Clock { get; }

    /// <summary>The IP address this test's requests come from.</summary>
    public string ClientIp { get; }

    /// <summary>Calls the API from <see cref="ClientIp"/>.</summary>
    public AuthApi Api { get; }

    /// <summary>Calls the same host from another IP address.</summary>
    public AuthApi FromAnotherIp() => new(_host, TestClientIpStartupFilter.NewIp());

    public ValueTask DisposeAsync() => _host.DisposeAsync();
}
