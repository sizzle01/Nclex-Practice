using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Nclex.Api.IntegrationTests.Infrastructure;

/// <summary>
/// A copy of the shared API host connected to the real Redis container, standing in for one API
/// replica. Two of these share Postgres and Redis like two replicas behind a load balancer.
/// Each gets its own client IP, so rate limits from other tests never apply.
/// </summary>
public sealed class RedisTestHost : IAsyncDisposable
{
    private readonly WebApplicationFactory<Program> _host;

    public RedisTestHost(ApiFactory factory)
    {
        _host = factory.WithWebHostBuilder(builder => builder.UseSetting("ConnectionStrings:Redis", factory.RedisConnectionString));
        Auth = new AuthApi(_host, TestClientIpStartupFilter.NewIp());
        Attempts = new AttemptsApi(factory, Auth);
        Notifications = new NotificationsApi(Auth);
    }

    public AuthApi Auth { get; }

    public AttemptsApi Attempts { get; }

    public NotificationsApi Notifications { get; }

    public IServiceProvider Services => _host.Services;

    public ValueTask DisposeAsync() => _host.DisposeAsync();
}
