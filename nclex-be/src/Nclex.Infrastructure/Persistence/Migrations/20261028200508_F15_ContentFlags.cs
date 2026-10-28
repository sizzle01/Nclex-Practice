using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Nclex.Domain.Flags;

#nullable disable

namespace Nclex.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class F15_ContentFlags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:attempt_status", "abandoned,in_progress,paused,submitted")
                .Annotation("Npgsql:Enum:cjmm_skill", "analyze_cues,evaluate_outcomes,generate_solutions,prioritize_hypotheses,recognize_cues,take_action")
                .Annotation("Npgsql:Enum:difficulty_tier", "difficult,easy,moderate")
                .Annotation("Npgsql:Enum:email_token_purpose", "change_email,reset_password,verify_email")
                .Annotation("Npgsql:Enum:exam_track", "PN,RN")
                .Annotation("Npgsql:Enum:flag_reason", "rationale_unclear,incorrect_answer_key,typo,outdated_guideline,media_problem,other")
                .Annotation("Npgsql:Enum:flag_status", "open,triaged,resolved,rejected")
                .Annotation("Npgsql:Enum:item_state", "answered,locked,seen,unseen")
                .Annotation("Npgsql:Enum:item_type", "bowtie,drag_drop_cloze,drag_drop_rationale,dropdown_cloze,dropdown_rationale,dropdown_table,fill_blank_calc,highlight_table,highlight_text,hotspot,matrix_mc,matrix_mr,multiple_choice,multiple_response_grouping,ordered_response,sata,select_n")
                .Annotation("Npgsql:Enum:notification_category", "content,engagement,reminders,results,security")
                .Annotation("Npgsql:Enum:notification_type", "draft_reminder,flag_resolved,goal_met,result_fail,result_pass,result_ready,security,streak,system")
                .Annotation("Npgsql:Enum:plan_tier", "free,premium")
                .Annotation("Npgsql:Enum:question_status", "active,retired")
                .Annotation("Npgsql:Enum:reveal_mode", "exam,tutor")
                .Annotation("Npgsql:Enum:submit_reason", "cat_stop,time_expired,user")
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
                .OldAnnotation("Npgsql:Enum:notification_category", "content,engagement,reminders,results,security")
                .OldAnnotation("Npgsql:Enum:notification_type", "draft_reminder,flag_resolved,goal_met,result_fail,result_pass,result_ready,security,streak,system")
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

            migrationBuilder.CreateTable(
                name: "question_flags",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    question_version_id = table.Column<Guid>(type: "uuid", nullable: false),
                    attempt_id = table.Column<Guid>(type: "uuid", nullable: true),
                    reason = table.Column<FlagReason>(type: "flag_reason", nullable: false),
                    comment = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<FlagStatus>(type: "flag_status", nullable: false, defaultValue: FlagStatus.Open),
                    assignee_id = table.Column<Guid>(type: "uuid", nullable: true),
                    resolution_note = table.Column<string>(type: "text", nullable: true),
                    resolved_by = table.Column<Guid>(type: "uuid", nullable: true),
                    resolved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_question_flags", x => x.id);
                    table.CheckConstraint("question_flags_comment_check", "char_length(comment) <= 2000");
                    table.ForeignKey(
                        name: "fk_question_flags_attempts_attempt_id",
                        column: x => x.attempt_id,
                        principalTable: "attempts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_question_flags_question_versions_question_version_id",
                        column: x => x.question_version_id,
                        principalTable: "question_versions",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_question_flags_users_assignee_id",
                        column: x => x.assignee_id,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_question_flags_users_resolved_by",
                        column: x => x.resolved_by,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_question_flags_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "flags_queue_idx",
                table: "question_flags",
                columns: new[] { "status", "created_at" },
                filter: "status IN ('open','triaged')");

            migrationBuilder.CreateIndex(
                name: "ix_question_flags_assignee_id",
                table: "question_flags",
                column: "assignee_id");

            migrationBuilder.CreateIndex(
                name: "ix_question_flags_attempt_id",
                table: "question_flags",
                column: "attempt_id");

            migrationBuilder.CreateIndex(
                name: "ix_question_flags_question_version_id",
                table: "question_flags",
                column: "question_version_id");

            migrationBuilder.CreateIndex(
                name: "ix_question_flags_resolved_by",
                table: "question_flags",
                column: "resolved_by");

            migrationBuilder.CreateIndex(
                name: "ix_question_flags_user_id_question_version_id_reason",
                table: "question_flags",
                columns: new[] { "user_id", "question_version_id", "reason" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "question_flags");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:attempt_status", "abandoned,in_progress,paused,submitted")
                .Annotation("Npgsql:Enum:cjmm_skill", "analyze_cues,evaluate_outcomes,generate_solutions,prioritize_hypotheses,recognize_cues,take_action")
                .Annotation("Npgsql:Enum:difficulty_tier", "difficult,easy,moderate")
                .Annotation("Npgsql:Enum:email_token_purpose", "change_email,reset_password,verify_email")
                .Annotation("Npgsql:Enum:exam_track", "PN,RN")
                .Annotation("Npgsql:Enum:item_state", "answered,locked,seen,unseen")
                .Annotation("Npgsql:Enum:item_type", "bowtie,drag_drop_cloze,drag_drop_rationale,dropdown_cloze,dropdown_rationale,dropdown_table,fill_blank_calc,highlight_table,highlight_text,hotspot,matrix_mc,matrix_mr,multiple_choice,multiple_response_grouping,ordered_response,sata,select_n")
                .Annotation("Npgsql:Enum:notification_category", "content,engagement,reminders,results,security")
                .Annotation("Npgsql:Enum:notification_type", "draft_reminder,flag_resolved,goal_met,result_fail,result_pass,result_ready,security,streak,system")
                .Annotation("Npgsql:Enum:plan_tier", "free,premium")
                .Annotation("Npgsql:Enum:question_status", "active,retired")
                .Annotation("Npgsql:Enum:reveal_mode", "exam,tutor")
                .Annotation("Npgsql:Enum:submit_reason", "cat_stop,time_expired,user")
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
                .OldAnnotation("Npgsql:Enum:flag_reason", "rationale_unclear,incorrect_answer_key,typo,outdated_guideline,media_problem,other")
                .OldAnnotation("Npgsql:Enum:flag_status", "open,triaged,resolved,rejected")
                .OldAnnotation("Npgsql:Enum:item_state", "answered,locked,seen,unseen")
                .OldAnnotation("Npgsql:Enum:item_type", "bowtie,drag_drop_cloze,drag_drop_rationale,dropdown_cloze,dropdown_rationale,dropdown_table,fill_blank_calc,highlight_table,highlight_text,hotspot,matrix_mc,matrix_mr,multiple_choice,multiple_response_grouping,ordered_response,sata,select_n")
                .OldAnnotation("Npgsql:Enum:notification_category", "content,engagement,reminders,results,security")
                .OldAnnotation("Npgsql:Enum:notification_type", "draft_reminder,flag_resolved,goal_met,result_fail,result_pass,result_ready,security,streak,system")
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
