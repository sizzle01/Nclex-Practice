using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using Nclex.Domain.Entities;

#nullable disable

namespace Nclex.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class F06_TestSets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Enum labels are listed in DDL order (database.md 2.1); EF lists them alphabetically.
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:cjmm_skill", "analyze_cues,evaluate_outcomes,generate_solutions,prioritize_hypotheses,recognize_cues,take_action")
                .Annotation("Npgsql:Enum:difficulty_tier", "difficult,easy,moderate")
                .Annotation("Npgsql:Enum:email_token_purpose", "change_email,reset_password,verify_email")
                .Annotation("Npgsql:Enum:exam_track", "PN,RN")
                .Annotation("Npgsql:Enum:item_type", "bowtie,drag_drop_cloze,drag_drop_rationale,dropdown_cloze,dropdown_rationale,dropdown_table,fill_blank_calc,highlight_table,highlight_text,hotspot,matrix_mc,matrix_mr,multiple_choice,multiple_response_grouping,ordered_response,sata,select_n")
                .Annotation("Npgsql:Enum:plan_tier", "free,premium")
                .Annotation("Npgsql:Enum:question_status", "active,retired")
                .Annotation("Npgsql:Enum:reveal_mode", "tutor,exam")
                .Annotation("Npgsql:Enum:test_mode", "practice,challenge,cat")
                .Annotation("Npgsql:Enum:test_set_kind", "prebuilt,custom,cat,assigned")
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
                .OldAnnotation("Npgsql:Enum:user_role", "admin,author,candidate,educator,reviewer")
                .OldAnnotation("Npgsql:Enum:version_status", "approved,draft,in_review,published,rejected,superseded")
                .OldAnnotation("Npgsql:PostgresExtension:citext", ",,")
                .OldAnnotation("Npgsql:PostgresExtension:pgcrypto", ",,");

            migrationBuilder.CreateTable(
                name: "test_sets",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    track = table.Column<ExamTrack>(type: "exam_track", nullable: false),
                    kind = table.Column<TestSetKind>(type: "test_set_kind", nullable: false),
                    owner_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    title = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    mode = table.Column<TestMode>(type: "test_mode", nullable: false, defaultValue: TestMode.Practice),
                    reveal_mode = table.Column<RevealMode>(type: "reveal_mode", nullable: false, defaultValue: RevealMode.Tutor),
                    timed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    per_question_sec = table.Column<int>(type: "integer", nullable: true),
                    time_budget_sec = table.Column<int>(type: "integer", nullable: true),
                    pass_threshold = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: true),
                    allow_back_nav = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    is_sample = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    access_tier = table.Column<PlanTier>(type: "plan_tier", nullable: false, defaultValue: PlanTier.Free),
                    is_published = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    generator_criteria = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    cat_config = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    question_count = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    // EF has no default for an owned JSON column, so the DDL default is added here by hand.
                    pause_policy = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'{\"maxPauses\":null,\"maxPauseSec\":null}'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_test_sets", x => x.id);
                    table.CheckConstraint("test_sets_check", "kind <> 'custom' OR owner_user_id IS NOT NULL");
                    table.CheckConstraint("test_sets_check1", "NOT timed OR per_question_sec IS NOT NULL OR time_budget_sec IS NOT NULL");
                    table.ForeignKey(
                        name: "fk_test_sets_users_owner_user_id",
                        column: x => x.owner_user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "test_set_items",
                columns: table => new
                {
                    test_set_id = table.Column<Guid>(type: "uuid", nullable: false),
                    position = table.Column<int>(type: "integer", nullable: false),
                    question_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_test_set_items", x => new { x.test_set_id, x.position });
                    table.ForeignKey(
                        name: "fk_test_set_items_questions_question_id",
                        column: x => x.question_id,
                        principalTable: "questions",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_test_set_items_test_sets_test_set_id",
                        column: x => x.test_set_id,
                        principalTable: "test_sets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_test_set_items_question_id",
                table: "test_set_items",
                column: "question_id");

            migrationBuilder.CreateIndex(
                name: "ix_test_set_items_test_set_id_question_id",
                table: "test_set_items",
                columns: new[] { "test_set_id", "question_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_test_sets_owner_user_id",
                table: "test_sets",
                column: "owner_user_id",
                filter: "kind = 'custom'");

            migrationBuilder.CreateIndex(
                name: "ix_test_sets_track_kind",
                table: "test_sets",
                columns: new[] { "track", "kind" },
                filter: "is_published");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "test_set_items");

            migrationBuilder.DropTable(
                name: "test_sets");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:cjmm_skill", "analyze_cues,evaluate_outcomes,generate_solutions,prioritize_hypotheses,recognize_cues,take_action")
                .Annotation("Npgsql:Enum:difficulty_tier", "difficult,easy,moderate")
                .Annotation("Npgsql:Enum:email_token_purpose", "change_email,reset_password,verify_email")
                .Annotation("Npgsql:Enum:exam_track", "PN,RN")
                .Annotation("Npgsql:Enum:item_type", "bowtie,drag_drop_cloze,drag_drop_rationale,dropdown_cloze,dropdown_rationale,dropdown_table,fill_blank_calc,highlight_table,highlight_text,hotspot,matrix_mc,matrix_mr,multiple_choice,multiple_response_grouping,ordered_response,sata,select_n")
                .Annotation("Npgsql:Enum:plan_tier", "free,premium")
                .Annotation("Npgsql:Enum:question_status", "active,retired")
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
        }
    }
}
