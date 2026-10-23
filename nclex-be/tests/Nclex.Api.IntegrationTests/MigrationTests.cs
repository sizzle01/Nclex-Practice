using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nclex.Api.IntegrationTests.Infrastructure;
using Nclex.Infrastructure.Persistence;

namespace Nclex.Api.IntegrationTests;

public sealed class MigrationTests(ApiFactory factory)
{
    [Theory]
    [InlineData("pgcrypto")]
    [InlineData("citext")]
    public async Task F00_migration_installs_required_extension(string extension)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var installed = await db.Database
            .SqlQuery<string>($"SELECT extname AS \"Value\" FROM pg_extension WHERE extname = {extension}")
            .ToListAsync(TestContext.Current.CancellationToken);

        installed.ShouldHaveSingleItem();
    }
}
