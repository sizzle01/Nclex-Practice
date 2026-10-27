using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using Nclex.Domain.Entities;

#nullable disable

namespace Nclex.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class F08_Attempts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Enum labels are listed in DDL order (database.md 2.1); EF lists them alphabetically.
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:attempt_status", "in_progress,paused,submitted,abandoned")
                .Annotation("Npgsql:Enum:cjmm_skill", "analyze_cues,evaluate_outcomes,generate_solutions,prioritize_hypotheses,recognize_cues,take_action")
                .Annotation("Npgsql:Enum:difficulty_tier", "difficult,easy,moderate")
                .Annotation("Npgsql:Enum:email_token_purpose", "change_email,reset_password,verify_email")
                .Annotation("Npgsql:Enum:exam_track", "PN,RN")
                .Annotation("Npgsql:Enum:item_state", "unseen,seen,answered,locked")
                .Annotation("Npgsql:Enum:item_type", "bowtie,drag_drop_cloze,drag_drop_rationale,dropdown_cloze,dropdown_rationale,dropdown_table,fill_blank_calc,highlight_table,highlight_text,hotspot,matrix_mc,matrix_mr,multiple_choice,multiple_response_grouping,ordered_response,sata,select_n")
                .Annotation("Npgsql:Enum:plan_tier", "free,premium")
                .Annotation("Npgsql:Enum:question_status", "active,retired")
                .Annotation("Npgsql:Enum:reveal_mode", "exam,tutor")
                .Annotation("Npgsql:Enum:submit_reason", "user,time_expired,cat_stop")
                .Annotation("Npgsql:Enum:test_mode", "cat,challenge,practice")
                .Annotation("Npgsql:Enum:test_set_kind", "assigned,cat,custom,prebuilt")
                .Annotation("Npgsql:Enum:user_role", "admin,author,candidate,educator,reviewer")
                .Annotation("Npgsql:Enum:version_status", "approved,draft,in_review,published,rejected,superseded")
                .Annotation("Npgsql:PostgresExtension:citext", ",,")
                .Annotation("Npgsql:PostgresExtension:pgcrypto", ",,")
                .OldAnnotation("Npgsql:Enum:cjmm_skill", "analyze_cues,evaluate_outcomes,generate_solutions,prioritize_hypotheses,recognize_cues,take_action")
                .OldAnnotation("Npgsql:Enum:difficulty_tier", "difficult,easy,moderate")
                .OldAnnotation("Npgsql:Enum:email_token_purpose", "change_email,reset_password,verify_email")
                .OldAnnotation("Npgsql:Enum:exam_track", "PN,RN")
                .OldAnnotation("Npgsql:Enum:item_type", "bowtie,drag_drop_cloze,drag_drop_rationale,dropdown_cloze,dropdown_rationale,dropdown_table,fill_blank_calc,highlight_table,highlight_text,hotspot,matrix_mc,matrix_mr,multiple_choice,multiple_response_grouping,ordered_response,sata,select_n")
                .OldAnnotation("Npgsql:Enum:plan_tier", "free,premium")
                .OldAnnotation("Npgsql:Enum:question_status", "active,retired")
                .OldAnnotation("Npgsql:Enum:reveal_mode", "exam,tutor")
                .OldAnnotation("Npgsql:Enum:test_mode", "cat,challenge,practice")
                .OldAnnotation("Npgsql:Enum:test_set_kind", "assigned,cat,custom,prebuilt")
                .OldAnnotation("Npgsql:Enum:user_role", "admin,author,candidate,educator,reviewer")
                .OldAnnotation("Npgsql:Enum:version_status", "approved,draft,in_review,published,rejected,superseded")
                .OldAnnotation("Npgsql:PostgresExtension:citext", ",,")
                .OldAnnotation("Npgsql:PostgresExtension:pgcrypto", ",,");

            migrationBuilder.CreateTable(
                name: "attempts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    test_set_id = table.Column<Guid>(type: "uuid", nullable: false),
                    track = table.Column<ExamTrack>(type: "exam_track", nullable: false),
                    status = table.Column<AttemptStatus>(type: "attempt_status", nullable: false, defaultValue: AttemptStatus.InProgress),
                    total_items = table.Column<int>(type: "integer", nullable: false),
                    current_position = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    exam_deadline_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    exam_remaining_ms = table.Column<long>(type: "bigint", nullable: true),
                    timer_epoch = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    pauses_used = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    pause_started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    pause_reason = table.Column<string>(type: "text", nullable: true),
                    total_paused_ms = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    active_client_id = table.Column<string>(type: "text", nullable: true),
                    lease_acquired_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    cat_state = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    last_activity_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    submitted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    submit_reason = table.Column<SubmitReason>(type: "submit_reason", nullable: true),
                    settings = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_attempts", x => x.id);
                    table.CheckConstraint("attempts_check", "status <> 'submitted' OR submitted_at IS NOT NULL");
                    table.CheckConstraint("attempts_check1", "status <> 'paused' OR exam_deadline_at IS NULL");
                    table.CheckConstraint("attempts_pause_reason_check", "pause_reason IN ('break','save_exit')");
                    table.ForeignKey(
                        name: "fk_attempts_test_sets_test_set_id",
                        column: x => x.test_set_id,
                        principalTable: "test_sets",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_attempts_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "attempt_items",
                columns: table => new
                {
                    attempt_id = table.Column<Guid>(type: "uuid", nullable: false),
                    position = table.Column<int>(type: "integer", nullable: false),
                    question_version_id = table.Column<Guid>(type: "uuid", nullable: false),
                    state = table.Column<ItemState>(type: "item_state", nullable: false, defaultValue: ItemState.Unseen),
                    response = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    client_seq = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    flagged = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    struck_options = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'[]'"),
                    entered_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    time_spent_ms = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    item_deadline_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    item_remaining_ms = table.Column<int>(type: "integer", nullable: true),
                    points_earned = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: true),
                    max_points = table.Column<short>(type: "smallint", nullable: true),
                    is_correct = table.Column<bool>(type: "boolean", nullable: true),
                    grade_detail = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    graded_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    locked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    answered_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_attempt_items", x => new { x.attempt_id, x.position });
                    table.ForeignKey(
                        name: "fk_attempt_items_attempts_attempt_id",
                        column: x => x.attempt_id,
                        principalTable: "attempts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_attempt_items_question_versions_question_version_id",
                        column: x => x.question_version_id,
                        principalTable: "question_versions",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_attempt_items_question_version_id",
                table: "attempt_items",
                column: "question_version_id");

            migrationBuilder.CreateIndex(
                name: "attempts_drafts_idx",
                table: "attempts",
                columns: new[] { "user_id", "last_activity_at" },
                descending: new[] { false, true },
                filter: "status IN ('in_progress','paused')");

            migrationBuilder.CreateIndex(
                name: "attempts_expiry_idx",
                table: "attempts",
                column: "exam_deadline_at",
                filter: "status = 'in_progress'");

            migrationBuilder.CreateIndex(
                name: "attempts_history_idx",
                table: "attempts",
                columns: new[] { "user_id", "submitted_at" },
                descending: new[] { false, true },
                filter: "status = 'submitted'");

            migrationBuilder.CreateIndex(
                name: "ix_attempts_test_set_id",
                table: "attempts",
                column: "test_set_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "attempt_items");

            migrationBuilder.DropTable(
                name: "attempts");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:cjmm_skill", "analyze_cues,evaluate_outcomes,generate_solutions,prioritize_hypotheses,recognize_cues,take_action")
                .Annotation("Npgsql:Enum:difficulty_tier", "difficult,easy,moderate")
                .Annotation("Npgsql:Enum:email_token_purpose", "change_email,reset_password,verify_email")
                .Annotation("Npgsql:Enum:exam_track", "PN,RN")
                .Annotation("Npgsql:Enum:item_type", "bowtie,drag_drop_cloze,drag_drop_rationale,dropdown_cloze,dropdown_rationale,dropdown_table,fill_blank_calc,highlight_table,highlight_text,hotspot,matrix_mc,matrix_mr,multiple_choice,multiple_response_grouping,ordered_response,sata,select_n")
                .Annotation("Npgsql:Enum:plan_tier", "free,premium")
                .Annotation("Npgsql:Enum:question_status", "active,retired")
                .Annotation("Npgsql:Enum:reveal_mode", "exam,tutor")
                .Annotation("Npgsql:Enum:test_mode", "cat,challenge,practice")
                .Annotation("Npgsql:Enum:test_set_kind", "assigned,cat,custom,prebuilt")
                .Annotation("Npgsql:Enum:user_role", "admin,author,candidate,educator,reviewer")
                .Annotation("Npgsql:Enum:version_status", "approved,draft,in_review,published,rejected,superseded")
                .Annotation("Npgsql:PostgresExtension:citext", ",,")
                .Annotation("Npgsql:PostgresExtension:pgcrypto", ",,")
                .OldAnnotation("Npgsql:Enum:attempt_status", "abandoned,in_progress,paused,submitted")
                .OldAnnotation("Npgsql:Enum:cjmm_skill", "analyze_cues,evaluate_outcomes,generate_solutions,prioritize_hypotheses,recognize_cues,take_action")
                .OldAnnotation("Npgsql:Enum:difficulty_tier", "difficult,easy,moderate")
                .OldAnnotation("Npgsql:Enum:email_token_purpose", "change_email,reset_password,verify_email")
                .OldAnnotation("Npgsql:Enum:exam_track", "PN,RN")
                .OldAnnotation("Npgsql:Enum:item_state", "answered,locked,seen,unseen")
                .OldAnnotation("Npgsql:Enum:item_type", "bowtie,drag_drop_cloze,drag_drop_rationale,dropdown_cloze,dropdown_rationale,dropdown_table,fill_blank_calc,highlight_table,highlight_text,hotspot,matrix_mc,matrix_mr,multiple_choice,multiple_response_grouping,ordered_response,sata,select_n")
                .OldAnnotation("Npgsql:Enum:plan_tier", "free,premium")
                .OldAnnotation("Npgsql:Enum:question_status", "active,retired")
                .OldAnnotation("Npgsql:Enum:reveal_mode", "exam,tutor")
                .OldAnnotation("Npgsql:Enum:submit_reason", "cat_stop,time_expired,user")
                .OldAnnotation("Npgsql:Enum:test_mode", "cat,challenge,practice")
                .OldAnnotation("Npgsql:Enum:test_set_kind", "assigned,cat,custom,prebuilt")
                .OldAnnotation("Npgsql:Enum:user_role", "admin,author,candidate,educator,reviewer")
                .OldAnnotation("Npgsql:Enum:version_status", "approved,draft,in_review,published,rejected,superseded")
                .OldAnnotation("Npgsql:PostgresExtension:citext", ",,")
                .OldAnnotation("Npgsql:PostgresExtension:pgcrypto", ",,");
        }
    }
}
