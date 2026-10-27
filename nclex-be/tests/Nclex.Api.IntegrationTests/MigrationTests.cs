using System.Runtime.CompilerServices;
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
    [InlineData("item_type", "multiple_choice,sata,select_n,matrix_mc,matrix_mr,multiple_response_grouping,dropdown_cloze,dropdown_table,dropdown_rationale,highlight_text,highlight_table,fill_blank_calc,ordered_response,drag_drop_cloze,drag_drop_rationale,bowtie,hotspot")]
    [InlineData("difficulty_tier", "easy,moderate,difficult")]
    [InlineData("cjmm_skill", "recognize_cues,analyze_cues,prioritize_hypotheses,generate_solutions,take_action,evaluate_outcomes")]
    [InlineData("version_status", "draft,in_review,approved,published,superseded,rejected")]
    [InlineData("question_status", "active,retired")]
    [InlineData("test_set_kind", "prebuilt,custom,cat,assigned")]
    [InlineData("test_mode", "practice,challenge,cat")]
    [InlineData("reveal_mode", "tutor,exam")]
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

    [Theory]
    [InlineData("id", "bigint", "nextval('audit_events_id_seq'::regclass)")]
    [InlineData("metadata", "jsonb", "'{}'::jsonb")]
    [InlineData("ip", "inet", null)]
    [InlineData("created_at", "timestamp with time zone", "now()")]
    public async Task F03_migration_creates_audit_events_as_in_the_ddl(string column, string type, string? defaultValue)
    {
        var rows = await QueryAsync(
            $"""
            SELECT data_type || '|' || coalesce(column_default, '') AS "Value" FROM information_schema.columns
            WHERE table_name = 'audit_events' AND column_name = {column}
            """);

        rows.ShouldHaveSingleItem().ShouldBe($"{type}|{defaultValue}");
    }

    [Fact]
    public async Task F03_migration_indexes_audit_events_by_entity()
    {
        var definition = await QueryAsync(
            $"SELECT indexdef AS \"Value\" FROM pg_indexes WHERE indexname = {"ix_audit_events_entity_type_entity_id"}");

        definition.ShouldHaveSingleItem().ShouldEndWith("(entity_type, entity_id)");
    }

    [Theory]
    [InlineData("qv_filter_idx", "WHERE (status = 'published'::version_status)")]
    [InlineData("qv_review_queue_idx", "WHERE (status = 'in_review'::version_status)")]
    [InlineData("qv_question_idx", "(question_id, version DESC)")]
    public async Task F04_migration_creates_the_question_version_indexes(string index, string ending)
    {
        var definition = await QueryAsync($"SELECT indexdef AS \"Value\" FROM pg_indexes WHERE indexname = {index}");

        definition.ShouldHaveSingleItem().ShouldEndWith(ending);
    }

    [Fact]
    public async Task F04_migration_links_questions_to_their_current_version()
    {
        var foreignKey = await QueryAsync(
            $"SELECT pg_get_constraintdef(oid) AS \"Value\" FROM pg_constraint WHERE conname = {"questions_current_version_fk"}");

        foreignKey.ShouldHaveSingleItem().ShouldBe("FOREIGN KEY (current_version_id) REFERENCES question_versions(id)");
    }

    [Theory]
    [InlineData("client_needs_categories", "21")]
    [InlineData("body_systems", "17")]
    public async Task F04_migration_moves_seeded_sequences_past_the_seed_rows(string table, string nextId)
    {
        // A sequence name can't be a query parameter, so it goes into the SQL text itself.
        var next = await QueryAsync(FormattableStringFactory.Create(
            $"SELECT (CASE WHEN is_called THEN last_value + 1 ELSE last_value END)::text AS \"Value\" FROM {table}_id_seq"));

        next.ShouldHaveSingleItem().ShouldBe(nextId);
    }

    [Theory]
    [InlineData("ix_test_sets_track_kind", "(track, kind) WHERE is_published")]
    [InlineData("ix_test_sets_owner_user_id", "(owner_user_id) WHERE (kind = 'custom'::test_set_kind)")]
    [InlineData("ix_test_set_items_test_set_id_question_id", "(test_set_id, question_id)")]
    public async Task F06_migration_creates_the_test_set_indexes(string index, string ending)
    {
        var definition = await QueryAsync($"SELECT indexdef AS \"Value\" FROM pg_indexes WHERE indexname = {index}");

        definition.ShouldHaveSingleItem().ShouldEndWith(ending);
    }

    [Theory]
    [InlineData("test_sets_check", "CHECK (((kind <> 'custom'::test_set_kind) OR (owner_user_id IS NOT NULL)))")]
    [InlineData("test_sets_check1", "CHECK (((NOT timed) OR (per_question_sec IS NOT NULL) OR (time_budget_sec IS NOT NULL)))")]
    public async Task F06_migration_adds_the_test_set_checks(string name, string definition)
    {
        var check = await QueryAsync($"SELECT pg_get_constraintdef(oid) AS \"Value\" FROM pg_constraint WHERE conname = {name}");

        check.ShouldHaveSingleItem().ShouldBe(definition);
    }

    [Theory]
    [InlineData("mode", "'practice'::test_mode")]
    [InlineData("reveal_mode", "'tutor'::reveal_mode")]
    [InlineData("pause_policy", "'{\"maxPauses\": null, \"maxPauseSec\": null}'::jsonb")]
    [InlineData("allow_back_nav", "true")]
    [InlineData("pass_threshold", null)]
    public async Task F06_migration_gives_test_sets_the_ddl_defaults(string column, string? defaultValue)
    {
        var rows = await QueryAsync(
            $"""
            SELECT coalesce(column_default, '') AS "Value" FROM information_schema.columns
            WHERE table_name = 'test_sets' AND column_name = {column}
            """);

        rows.ShouldHaveSingleItem().ShouldBe(defaultValue ?? "");
    }

    [Theory]
    [InlineData("times_seen", "integer|0")]
    [InlineData("last_correct", "boolean|")]
    [InlineData("last_seen_at", "timestamp with time zone|")]
    public async Task F07_migration_creates_user_question_history_as_in_the_ddl(string column, string typeAndDefault)
    {
        var rows = await QueryAsync(
            $"""
            SELECT data_type || '|' || coalesce(column_default, '') AS "Value" FROM information_schema.columns
            WHERE table_name = 'user_question_history' AND column_name = {column}
            """);

        rows.ShouldHaveSingleItem().ShouldBe(typeAndDefault);
    }

    [Fact]
    public async Task F07_migration_removes_history_with_the_user()
    {
        var foreignKey = await QueryAsync(
            $"SELECT pg_get_constraintdef(oid) AS \"Value\" FROM pg_constraint WHERE conname = {"fk_user_question_history_users_user_id"}");

        foreignKey.ShouldHaveSingleItem().ShouldBe("FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE");
    }

    private async Task<List<string>> QueryAsync(FormattableString sql)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await db.Database.SqlQuery<string>(sql).ToListAsync(TestContext.Current.CancellationToken);
    }
}
