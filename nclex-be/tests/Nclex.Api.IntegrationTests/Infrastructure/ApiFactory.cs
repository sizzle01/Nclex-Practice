using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nclex.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace Nclex.Api.IntegrationTests.Infrastructure;

/// <summary>
/// Hosts the real API in memory against a throwaway Postgres 16 container.
/// Shared by the whole test assembly (see AssemblyInfo.cs) so the container starts once.
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    // Nothing listens on port 1, so Redis is always unreachable: tests stay independent of any
    // Redis on the developer's machine and cover the "Redis down means degraded" rule.
    private const string UnreachableRedis = "127.0.0.1:1,connectTimeout=200";

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16").Build();

    public async ValueTask InitializeAsync()
    {
        await _postgres.StartAsync();

        // Applying the real migrations proves they run cleanly on a fresh database.
        await using var scope = Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Postgres", _postgres.GetConnectionString());
        builder.UseSetting("ConnectionStrings:Redis", UnreachableRedis);

        // Adds test-only controllers (e.g. ThrowingController) from this assembly.
        builder.ConfigureTestServices(services =>
            services.AddControllers().AddApplicationPart(typeof(ApiFactory).Assembly));
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _postgres.DisposeAsync();
    }
}
