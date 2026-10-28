using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Nclex.Domain.Notifications;

#nullable disable

namespace Nclex.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class F12_Notifications : Migration
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
                .Annotation("Npgsql:Enum:item_state", "answered,locked,seen,unseen")
                .Annotation("Npgsql:Enum:item_type", "bowtie,drag_drop_cloze,drag_drop_rationale,dropdown_cloze,dropdown_rationale,dropdown_table,fill_blank_calc,highlight_table,highlight_text,hotspot,matrix_mc,matrix_mr,multiple_choice,multiple_response_grouping,ordered_response,sata,select_n")
                .Annotation("Npgsql:Enum:notification_category", "results,reminders,engagement,content,security")
                .Annotation("Npgsql:Enum:notification_type", "result_pass,result_fail,result_ready,draft_reminder,streak,goal_met,flag_resolved,security,system")
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
                name: "notification_preferences",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    category = table.Column<NotificationCategory>(type: "notification_category", nullable: false),
                    in_app = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    email = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notification_preferences", x => new { x.user_id, x.category });
                    table.ForeignKey(
                        name: "fk_notification_preferences_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<NotificationType>(type: "notification_type", nullable: false),
                    category = table.Column<NotificationCategory>(type: "notification_category", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    body = table.Column<string>(type: "text", nullable: false),
                    data = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'{}'"),
                    related_attempt_id = table.Column<Guid>(type: "uuid", nullable: true),
                    channels = table.Column<string[]>(type: "text[]", nullable: false),
                    read_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notifications", x => x.id);
                    table.ForeignKey(
                        name: "fk_notifications_attempts_related_attempt_id",
                        column: x => x.related_attempt_id,
                        principalTable: "attempts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_notifications_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "notif_feed_idx",
                table: "notifications",
                columns: new[] { "user_id", "created_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "notif_result_once",
                table: "notifications",
                column: "related_attempt_id",
                unique: true,
                filter: "type IN ('result_pass','result_fail','result_ready')");

            migrationBuilder.CreateIndex(
                name: "notif_unread_idx",
                table: "notifications",
                column: "user_id",
                filter: "read_at IS NULL");

            // Existing accounts get the same defaults as new ones (database.md section 2.8):
            // results and security by email too, everything else in-app only.
            migrationBuilder.Sql(
                """
                INSERT INTO notification_preferences (user_id, category, in_app, email)
                SELECT u.id, c.category, true, c.category IN ('results', 'security')
                FROM users u
                CROSS JOIN unnest(enum_range(NULL::notification_category)) AS c(category)
                ON CONFLICT DO NOTHING;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "notification_preferences");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:attempt_status", "abandoned,in_progress,paused,submitted")
                .Annotation("Npgsql:Enum:cjmm_skill", "analyze_cues,evaluate_outcomes,generate_solutions,prioritize_hypotheses,recognize_cues,take_action")
                .Annotation("Npgsql:Enum:difficulty_tier", "difficult,easy,moderate")
                .Annotation("Npgsql:Enum:email_token_purpose", "change_email,reset_password,verify_email")
                .Annotation("Npgsql:Enum:exam_track", "PN,RN")
                .Annotation("Npgsql:Enum:item_state", "answered,locked,seen,unseen")
                .Annotation("Npgsql:Enum:item_type", "bowtie,drag_drop_cloze,drag_drop_rationale,dropdown_cloze,dropdown_rationale,dropdown_table,fill_blank_calc,highlight_table,highlight_text,hotspot,matrix_mc,matrix_mr,multiple_choice,multiple_response_grouping,ordered_response,sata,select_n")
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
        }
    }
}
