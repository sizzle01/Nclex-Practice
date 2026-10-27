using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Nclex.Application.Abstractions;
using Nclex.Infrastructure.Email;
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

    /// <summary>Signing key for access tokens in tests. Tests use it to forge expired or tampered tokens.</summary>
    public const string JwtSigningKey = "integration-tests-signing-key-0123456789abcdef";

    /// <summary>Frontend address used in emailed links during tests.</summary>
    public const string FrontendBaseUrl = "https://app.test";

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16").Build();

    /// <summary>Stands in for SMTP when tests run the outbox dispatcher.</summary>
    public FakeEmailSender EmailSender { get; } = new();

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
        builder.UseSetting("Jwt:SigningKey", JwtSigningKey);
        builder.UseSetting("Email:From", "NCLEX Practice <no-reply@nclex-practice.test>");
        builder.UseSetting("Email:FrontendBaseUrl", FrontendBaseUrl);
        builder.UseSetting("Email:Smtp:Host", "localhost");

        builder.ConfigureTestServices(services =>
        {
            // Adds test-only controllers (e.g. ThrowingController) from this assembly.
            services.AddControllers().AddApplicationPart(typeof(ApiFactory).Assembly);

            // The worker's email services, so tests can run OutboxDispatcher against this database.
            services.AddEmailDelivery();
            services.Replace(ServiceDescriptor.Singleton<IEmailSender>(EmailSender));
        });
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _postgres.DisposeAsync();
    }
}
