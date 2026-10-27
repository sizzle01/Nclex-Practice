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
    [InlineData("email_token_purpose", "verify_email,reset_password,change_email")]
    public async Task Migrations_create_enums_with_labels_in_ddl_order(string enumName, string labels)
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

    [Theory]
    [InlineData("outbox_pending_idx", "WHERE (status = 'pending'::text)")]
    [InlineData("ix_email_tokens_user_id_purpose", "WHERE (consumed_at IS NULL)")]
    public async Task F02_migration_creates_the_partial_indexes(string index, string filter)
    {
        var definition = await QueryAsync($"SELECT indexdef AS \"Value\" FROM pg_indexes WHERE indexname = {index}");

        definition.ShouldHaveSingleItem().ShouldEndWith(filter);
    }

    [Fact]
    public async Task F02_migration_restricts_outbox_status_values()
    {
        var check = await QueryAsync(
            $"SELECT pg_get_constraintdef(oid) AS \"Value\" FROM pg_constraint WHERE conname = {"email_outbox_status_check"}");

        check.ShouldHaveSingleItem().ShouldContain("'pending'::text, 'sending'::text, 'sent'::text, 'failed'::text");
    }

    private async Task<List<string>> QueryAsync(FormattableString sql)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await db.Database.SqlQuery<string>(sql).ToListAsync(TestContext.Current.CancellationToken);
    }
}
