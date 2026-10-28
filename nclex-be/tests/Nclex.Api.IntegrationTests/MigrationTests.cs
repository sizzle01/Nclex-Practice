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
    [InlineData("attempt_status", "in_progress,paused,submitted,abandoned")]
    [InlineData("submit_reason", "user,time_expired,cat_stop")]
    [InlineData("item_state", "unseen,seen,answered,locked")]
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

    [Theory]
    [InlineData("attempts_drafts_idx", "(user_id, last_activity_at DESC) WHERE (status = ANY (ARRAY['in_progress'::attempt_status, 'paused'::attempt_status]))")]
    [InlineData("attempts_history_idx", "(user_id, submitted_at DESC) WHERE (status = 'submitted'::attempt_status)")]
    [InlineData("attempts_expiry_idx", "(exam_deadline_at) WHERE (status = 'in_progress'::attempt_status)")]
    [InlineData("pk_attempt_items", "(attempt_id, \"position\")")]
    public async Task F08_migration_creates_the_attempt_indexes(string index, string ending)
    {
        var definition = await QueryAsync($"SELECT indexdef AS \"Value\" FROM pg_indexes WHERE indexname = {index}");

        definition.ShouldHaveSingleItem().ShouldEndWith(ending);
    }

    [Theory]
    [InlineData("attempts_check", "CHECK (((status <> 'submitted'::attempt_status) OR (submitted_at IS NOT NULL)))")]
    [InlineData("attempts_check1", "CHECK (((status <> 'paused'::attempt_status) OR (exam_deadline_at IS NULL)))")]
    [InlineData("attempts_pause_reason_check", "CHECK ((pause_reason = ANY (ARRAY['break'::text, 'save_exit'::text])))")]
    public async Task F08_migration_adds_the_attempt_checks(string name, string definition)
    {
        var check = await QueryAsync($"SELECT pg_get_constraintdef(oid) AS \"Value\" FROM pg_constraint WHERE conname = {name}");

        check.ShouldHaveSingleItem().ShouldBe(definition);
    }

    [Theory]
    [InlineData("attempts", "status", "'in_progress'::attempt_status")]
    [InlineData("attempts", "current_position", "1")]
    [InlineData("attempts", "timer_epoch", "0")]
    [InlineData("attempts", "pauses_used", "0")]
    [InlineData("attempts", "total_paused_ms", "0")]
    [InlineData("attempts", "settings", "")]
    [InlineData("attempt_items", "state", "'unseen'::item_state")]
    [InlineData("attempt_items", "client_seq", "0")]
    [InlineData("attempt_items", "flagged", "false")]
    [InlineData("attempt_items", "struck_options", "'[]'::jsonb")]
    [InlineData("attempt_items", "time_spent_ms", "0")]
    public async Task F08_migration_gives_attempt_columns_the_ddl_defaults(string table, string column, string defaultValue)
    {
        var rows = await QueryAsync(
            $"""
            SELECT coalesce(column_default, '') AS "Value" FROM information_schema.columns
            WHERE table_name = {table} AND column_name = {column}
            """);

        rows.ShouldHaveSingleItem().ShouldBe(defaultValue);
    }

    [Fact]
    public async Task F08_migration_deletes_items_with_their_attempt()
    {
        var rule = await QueryAsync(
            $"SELECT confdeltype::text AS \"Value\" FROM pg_constraint WHERE conname = {"fk_attempt_items_attempts_attempt_id"}");

        rule.ShouldHaveSingleItem().ShouldBe("c");
    }

    [Theory]
    [InlineData("attempt_results", "points_earned", "numeric(8,2)|NO|")]
    [InlineData("attempt_results", "max_points", "numeric(8,2)|NO|")]
    [InlineData("attempt_results", "score_pct", "numeric(5,4)|NO|")]
    [InlineData("attempt_results", "items_correct", "integer|NO|")]
    [InlineData("attempt_results", "items_answered", "integer|NO|")]
    [InlineData("attempt_results", "items_total", "integer|NO|")]
    [InlineData("attempt_results", "total_time_ms", "bigint|NO|")]
    [InlineData("attempt_results", "avg_time_ms", "integer|NO|")]
    [InlineData("attempt_results", "pass_threshold", "numeric(5,4)|YES|")]
    [InlineData("attempt_results", "passed", "boolean|YES|")]
    [InlineData("attempt_results", "readiness_label", "text|YES|")]
    [InlineData("attempt_results", "cat_theta", "numeric(6,3)|YES|")]
    [InlineData("attempt_results", "cat_se", "numeric(6,3)|YES|")]
    [InlineData("attempt_results", "graded_at", "timestamp with time zone|NO|now()")]
    [InlineData("attempt_category_scores", "dimension", "text|NO|")]
    [InlineData("attempt_category_scores", "dimension_key", "text|NO|")]
    [InlineData("attempt_category_scores", "items", "integer|NO|")]
    [InlineData("attempt_category_scores", "items_correct", "integer|NO|")]
    [InlineData("attempt_category_scores", "points_earned", "numeric(8,2)|NO|")]
    [InlineData("attempt_category_scores", "max_points", "numeric(8,2)|NO|")]
    [InlineData("attempt_category_scores", "time_ms", "bigint|NO|")]
    public async Task F10_migration_creates_the_result_tables_as_in_the_ddl(string table, string column, string typeNullableDefault)
    {
        var rows = await QueryAsync(
            $"""
            SELECT format_type(a.atttypid, a.atttypmod) || '|' || c.is_nullable || '|' || coalesce(c.column_default, '') AS "Value"
            FROM information_schema.columns c
            JOIN pg_attribute a ON a.attrelid = c.table_name::regclass AND a.attname = c.column_name
            WHERE c.table_name = {table} AND c.column_name = {column}
            """);

        rows.ShouldHaveSingleItem().ShouldBe(typeNullableDefault);
    }

    [Theory]
    [InlineData("pk_attempt_results", "(attempt_id)")]
    [InlineData("pk_attempt_category_scores", "(attempt_id, dimension, dimension_key)")]
    public async Task F10_migration_keys_the_result_tables_by_attempt(string index, string ending)
    {
        var definition = await QueryAsync($"SELECT indexdef AS \"Value\" FROM pg_indexes WHERE indexname = {index}");

        definition.ShouldHaveSingleItem().ShouldEndWith(ending);
    }

    [Fact]
    public async Task F10_migration_restricts_the_breakdown_dimensions()
    {
        var check = await QueryAsync(
            $"SELECT pg_get_constraintdef(oid) AS \"Value\" FROM pg_constraint WHERE conname = {"attempt_category_scores_dimension_check"}");

        check.ShouldHaveSingleItem().ShouldBe(
            "CHECK ((dimension = ANY (ARRAY['client_needs'::text, 'client_needs_parent'::text, 'body_system'::text, 'item_type'::text, 'cjmm_skill'::text, 'difficulty'::text])))");
    }

    [Theory]
    [InlineData("fk_attempt_results_attempts_attempt_id")]
    [InlineData("fk_attempt_category_scores_attempts_attempt_id")]
    public async Task F10_migration_deletes_results_with_their_attempt(string foreignKey)
    {
        var rule = await QueryAsync($"SELECT confdeltype::text AS \"Value\" FROM pg_constraint WHERE conname = {foreignKey}");

        rule.ShouldHaveSingleItem().ShouldBe("c");
    }

    private async Task<List<string>> QueryAsync(FormattableString sql)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await db.Database.SqlQuery<string>(sql).ToListAsync(TestContext.Current.CancellationToken);
    }
}
