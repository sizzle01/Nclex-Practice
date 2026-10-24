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
        var installed = await QueryAsync($"SELECT extname AS \"Value\" FROM pg_extension WHERE extname = {extension}");

        installed.ShouldHaveSingleItem();
    }

    [Theory]
    [InlineData("exam_track", "RN,PN")]
    [InlineData("user_role", "candidate,educator,author,reviewer,admin")]
    [InlineData("plan_tier", "free,premium")]
    public async Task F01_migration_creates_enums_with_labels_in_ddl_order(string enumName, string labels)
    {
        var actual = await QueryAsync(
            $"""
            SELECT e.enumlabel AS "Value" FROM pg_enum e JOIN pg_type t ON t.oid = e.enumtypid
            WHERE t.typname = {enumName} ORDER BY e.enumsortorder
            """);

        string.Join(',', actual).ShouldBe(labels);
    }

    [Fact]
    public async Task F01_migration_creates_the_partial_index_on_active_sessions()
    {
        var definition = await QueryAsync(
            $"SELECT indexdef AS \"Value\" FROM pg_indexes WHERE indexname = {"ix_refresh_sessions_user_id"}");

        definition.ShouldHaveSingleItem().ShouldEndWith("WHERE (revoked_at IS NULL)");
    }

    private async Task<List<string>> QueryAsync(FormattableString sql)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await db.Database.SqlQuery<string>(sql).ToListAsync(TestContext.Current.CancellationToken);
    }
}
