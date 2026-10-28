using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Nclex.Application.Abstractions;

namespace Nclex.Api.IntegrationTests.Infrastructure;

/// <summary>
/// A copy of the shared API host with a manual clock and a recording job scheduler, for timer,
/// pause and expiry tests. It uses the same Postgres database. Each test gets its own host and clock.
/// </summary>
public sealed class TimedTestHost : IAsyncDisposable
{
    private readonly WebApplicationFactory<Program> _host;

    public TimedTestHost(ApiFactory factory)
    {
        // Starts at the real time, so access tokens issued now still pass JWT validation.
        Clock = new ManualTimeProvider(DateTimeOffset.UtcNow);
        _host = factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            services.Replace(ServiceDescriptor.Singleton<TimeProvider>(Clock));
            services.Replace(ServiceDescriptor.Singleton<IJobScheduler>(Jobs));
        }));
        Auth = new AuthApi(_host);
        Attempts = new AttemptsApi(factory, Auth);
    }

    public ManualTimeProvider Clock { get; }

    public RecordingJobScheduler Jobs { get; } = new();

    /// <summary>Calls this host as a signed-in user, or anonymously.</summary>
    public AuthApi Auth { get; }

    /// <summary>Calls the attempt endpoints of this host.</summary>
    public AttemptsApi Attempts { get; }

    /// <summary>Runs <paramref name="work"/> with a service from this host, which sees the manual clock.</summary>
    public async Task<TResult> WithServiceAsync<TService, TResult>(Func<TService, Task<TResult>> work)
        where TService : notnull
    {
        await using var scope = _host.Services.CreateAsyncScope();
        return await work(scope.ServiceProvider.GetRequiredService<TService>());
    }

    public ValueTask DisposeAsync() => _host.DisposeAsync();
}
