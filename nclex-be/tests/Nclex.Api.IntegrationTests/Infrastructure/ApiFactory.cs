using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Nclex.Application.Abstractions;
using Nclex.Domain.Entities;
using Nclex.Infrastructure.Email;
using Nclex.Infrastructure.Persistence;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace Nclex.Api.IntegrationTests.Infrastructure;

/// <summary>
/// Hosts the real API in memory against a throwaway Postgres 16 container.
/// Shared by the whole test assembly (see AssemblyInfo.cs) so the container starts once.
/// This host has no Redis; tests that need it (rate limits, lockout) use <see cref="SecurityTestHost"/>,
/// which points a copy of this host at the Redis container started here.
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    // Nothing listens on port 1, so Redis is always unreachable here: tests stay independent of any
    // Redis on the developer's machine and cover the "Redis down means degraded" rule. It also
    // keeps rate limits out of tests that log in many times from the same (missing) client IP.
    private const string UnreachableRedis = "127.0.0.1:1,connectTimeout=200";

    /// <summary>Signing key for access tokens in tests. Tests use it to forge expired or tampered tokens.</summary>
    public const string JwtSigningKey = "integration-tests-signing-key-0123456789abcdef";

    /// <summary>Signing key for media links in tests.</summary>
    public const string MediaSigningKey = "integration-tests-media-signing-key-0123456789abcdef";

    /// <summary>Frontend address used in emailed links during tests.</summary>
    public const string FrontendBaseUrl = "https://app.test";

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16").Build();
    private readonly RedisContainer _redis = new RedisBuilder("redis:7").Build();

    /// <summary>Stands in for SMTP when tests run the outbox dispatcher.</summary>
    public FakeEmailSender EmailSender { get; } = new();

    /// <summary>A fresh folder for this test run's media files; removed when the run ends.</summary>
    public string MediaRoot { get; } = Path.Combine(Path.GetTempPath(), $"nclex-test-media-{Guid.NewGuid():N}");

    /// <summary>Connection string of the real Redis container, for <see cref="SecurityTestHost"/>.</summary>
    public string RedisConnectionString => _redis.GetConnectionString();

    public async ValueTask InitializeAsync()
    {
        await Task.WhenAll(_postgres.StartAsync(), _redis.StartAsync());

        // Applying the real migrations proves they run cleanly on a fresh database.
        await using var scope = Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();

        // The importer creates its author account on first use. Two first imports running at once
        // would both try to create it, so tests that import in parallel start with it in place.
        db.Users.Add(User.CreateContentImporter(DateTimeOffset.UtcNow));
        await db.SaveChangesAsync();
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
        builder.UseSetting("MediaStorage:LocalRoot", MediaRoot);
        builder.UseSetting("MediaStorage:SigningKey", MediaSigningKey);

        builder.ConfigureTestServices(services =>
        {
            // Adds test-only controllers (e.g. ThrowingController) from this assembly.
            services.AddControllers().AddApplicationPart(typeof(ApiFactory).Assembly);

            // The worker's email services, so tests can run OutboxDispatcher against this database.
            services.AddEmailDelivery();
            services.Replace(ServiceDescriptor.Singleton<IEmailSender>(EmailSender));

            // Lets a test choose its client IP address (TestServer has none).
            services.AddSingleton<IStartupFilter, TestClientIpStartupFilter>();
        });
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _postgres.DisposeAsync();
        await _redis.DisposeAsync();
        if (Directory.Exists(MediaRoot))
        {
            Directory.Delete(MediaRoot, recursive: true);
        }
    }
}
