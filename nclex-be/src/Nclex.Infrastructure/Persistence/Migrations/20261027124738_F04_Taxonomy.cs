using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using Nclex.Domain.Entities;
using Nclex.Domain.ItemTypes;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Nclex.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class F04_Taxonomy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Enum labels are listed in DDL order (database.md 2.1); EF lists them alphabetically.
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:cjmm_skill", "recognize_cues,analyze_cues,prioritize_hypotheses,generate_solutions,take_action,evaluate_outcomes")
                .Annotation("Npgsql:Enum:difficulty_tier", "easy,moderate,difficult")
                .Annotation("Npgsql:Enum:email_token_purpose", "change_email,reset_password,verify_email")
                .Annotation("Npgsql:Enum:exam_track", "PN,RN")
                .Annotation("Npgsql:Enum:item_type", "multiple_choice,sata,select_n,matrix_mc,matrix_mr,multiple_response_grouping,dropdown_cloze,dropdown_table,dropdown_rationale,highlight_text,highlight_table,fill_blank_calc,ordered_response,drag_drop_cloze,drag_drop_rationale,bowtie,hotspot")
                .Annotation("Npgsql:Enum:plan_tier", "free,premium")
                .Annotation("Npgsql:Enum:question_status", "active,retired")
                .Annotation("Npgsql:Enum:user_role", "admin,author,candidate,educator,reviewer")
                .Annotation("Npgsql:Enum:version_status", "draft,in_review,approved,published,superseded,rejected")
                .Annotation("Npgsql:PostgresExtension:citext", ",,")
                .Annotation("Npgsql:PostgresExtension:pgcrypto", ",,")
                .OldAnnotation("Npgsql:Enum:email_token_purpose", "change_email,reset_password,verify_email")
                .OldAnnotation("Npgsql:Enum:exam_track", "PN,RN")
                .OldAnnotation("Npgsql:Enum:plan_tier", "free,premium")
                .OldAnnotation("Npgsql:Enum:user_role", "admin,author,candidate,educator,reviewer")
                .OldAnnotation("Npgsql:PostgresExtension:citext", ",,")
                .OldAnnotation("Npgsql:PostgresExtension:pgcrypto", ",,");

            migrationBuilder.CreateTable(
                name: "body_systems",
                columns: table => new
                {
                    id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    slug = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_body_systems", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "case_studies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    track = table.Column<ExamTrack>(type: "exam_track", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    scenario = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_case_studies", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "client_needs_categories",
                columns: table => new
                {
                    id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    track = table.Column<ExamTrack>(type: "exam_track", nullable: false),
                    parent_id = table.Column<short>(type: "smallint", nullable: true),
                    code = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    weight_min = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    weight_max = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    sort_order = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_client_needs_categories", x => x.id);
                    table.ForeignKey(
                        name: "fk_client_needs_categories_client_needs_categories_parent_id",
                        column: x => x.parent_id,
                        principalTable: "client_needs_categories",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "media_assets",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    kind = table.Column<string>(type: "text", nullable: false),
                    storage_key = table.Column<string>(type: "text", nullable: false),
                    mime_type = table.Column<string>(type: "text", nullable: false),
                    bytes = table.Column<long>(type: "bigint", nullable: false),
                    width = table.Column<int>(type: "integer", nullable: true),
                    height = table.Column<int>(type: "integer", nullable: true),
                    duration_ms = table.Column<int>(type: "integer", nullable: true),
                    alt_text = table.Column<string>(type: "text", nullable: true),
                    captions_key = table.Column<string>(type: "text", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_media_assets", x => x.id);
                    table.CheckConstraint("media_assets_kind_check", "kind IN ('image','audio','video')");
                    table.ForeignKey(
                        name: "fk_media_assets_users_created_by",
                        column: x => x.created_by,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "question_banks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    track = table.Column<ExamTrack>(type: "exam_track", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    access_tier = table.Column<PlanTier>(type: "plan_tier", nullable: false, defaultValue: PlanTier.Free),
                    is_published = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_question_banks", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "question_bank_items",
                columns: table => new
                {
                    bank_id = table.Column<Guid>(type: "uuid", nullable: false),
                    question_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_question_bank_items", x => new { x.bank_id, x.question_id });
                    table.ForeignKey(
                        name: "fk_question_bank_items_question_banks_bank_id",
                        column: x => x.bank_id,
                        principalTable: "question_banks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "question_versions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    question_id = table.Column<Guid>(type: "uuid", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<VersionStatus>(type: "version_status", nullable: false, defaultValue: VersionStatus.Draft),
                    item_type = table.Column<ItemType>(type: "item_type", nullable: false),
                    stem = table.Column<string>(type: "text", nullable: false),
                    content = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    answer_key = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    rationale = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    educational_objective = table.Column<string>(type: "text", nullable: false),
                    exhibits = table.Column<JsonDocument>(type: "jsonb", nullable: false, defaultValueSql: "'[]'"),
                    client_needs_id = table.Column<short>(type: "smallint", nullable: false),
                    body_system_id = table.Column<short>(type: "smallint", nullable: false),
                    cjmm_skill = table.Column<CjmmSkill>(type: "cjmm_skill", nullable: true),
                    difficulty = table.Column<DifficultyTier>(type: "difficulty_tier", nullable: false),
                    irt_a = table.Column<decimal>(type: "numeric(6,3)", precision: 6, scale: 3, nullable: true),
                    irt_b = table.Column<decimal>(type: "numeric(6,3)", precision: 6, scale: 3, nullable: true),
                    irt_c = table.Column<decimal>(type: "numeric(6,3)", precision: 6, scale: 3, nullable: true),
                    time_allotment_sec = table.Column<int>(type: "integer", nullable: false, defaultValue: 120),
                    max_points = table.Column<short>(type: "smallint", nullable: false),
                    case_study_id = table.Column<Guid>(type: "uuid", nullable: true),
                    case_position = table.Column<short>(type: "smallint", nullable: true),
                    author_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reviewer_id = table.Column<Guid>(type: "uuid", nullable: true),
                    review_notes = table.Column<string>(type: "text", nullable: true),
                    reviewed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    published_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_question_versions", x => x.id);
                    table.CheckConstraint("question_versions_case_study_check", "(case_study_id IS NULL) = (case_position IS NULL)");
                    table.CheckConstraint("question_versions_max_points_check", "max_points > 0");
                    table.CheckConstraint("question_versions_time_allotment_sec_check", "time_allotment_sec > 0");
                    table.ForeignKey(
                        name: "fk_question_versions_body_systems_body_system_id",
                        column: x => x.body_system_id,
                        principalTable: "body_systems",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_question_versions_case_studies_case_study_id",
                        column: x => x.case_study_id,
                        principalTable: "case_studies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_question_versions_client_needs_categories_client_needs_id",
                        column: x => x.client_needs_id,
                        principalTable: "client_needs_categories",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_question_versions_users_author_id",
                        column: x => x.author_id,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_question_versions_users_reviewer_id",
                        column: x => x.reviewer_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "questions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    track = table.Column<ExamTrack>(type: "exam_track", nullable: false),
                    status = table.Column<QuestionStatus>(type: "question_status", nullable: false, defaultValue: QuestionStatus.Active),
                    current_version_id = table.Column<Guid>(type: "uuid", nullable: true),
                    access_tier = table.Column<PlanTier>(type: "plan_tier", nullable: false, defaultValue: PlanTier.Free),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    retired_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_questions", x => x.id);
                    table.ForeignKey(
                        name: "fk_questions_users_created_by",
                        column: x => x.created_by,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "questions_current_version_fk",
                        column: x => x.current_version_id,
                        principalTable: "question_versions",
                        principalColumn: "id");
                });

            migrationBuilder.InsertData(
                table: "body_systems",
                columns: new[] { "id", "name", "slug" },
                values: new object[,]
                {
                    { (short)1, "Cardiovascular", "cardiovascular" },
                    { (short)2, "Respiratory", "respiratory" },
                    { (short)3, "Neurological", "neurological" },
                    { (short)4, "Gastrointestinal", "gastrointestinal" },
                    { (short)5, "Renal and Urinary", "renal-urinary" },
                    { (short)6, "Endocrine", "endocrine" },
                    { (short)7, "Musculoskeletal", "musculoskeletal" },
                    { (short)8, "Integumentary", "integumentary" },
                    { (short)9, "Hematologic and Oncologic", "hematologic-oncologic" },
                    { (short)10, "Immune", "immune" },
                    { (short)11, "Reproductive", "reproductive" },
                    { (short)12, "Maternal and Newborn", "maternal-newborn" },
                    { (short)13, "Pediatrics", "pediatrics" },
                    { (short)14, "Mental Health", "mental-health" },
                    { (short)15, "Pharmacology", "pharmacology" },
                    { (short)16, "Fundamentals", "fundamentals" }
                });

            migrationBuilder.InsertData(
                table: "client_needs_categories",
                columns: new[] { "id", "code", "name", "parent_id", "sort_order", "track", "weight_max", "weight_min" },
                values: new object[,]
                {
                    { (short)1, "SECE", "Safe & Effective Care Environment", null, (short)1, ExamTrack.RN, null, null },
                    { (short)4, "HPM", "Health Promotion and Maintenance", null, (short)2, ExamTrack.RN, 12m, 6m },
                    { (short)5, "PSI", "Psychosocial Integrity", null, (short)3, ExamTrack.RN, 12m, 6m },
                    { (short)6, "PHI", "Physiological Integrity", null, (short)4, ExamTrack.RN, null, null },
                    { (short)11, "SECE", "Safe & Effective Care Environment", null, (short)1, ExamTrack.PN, null, null },
                    { (short)14, "HPM", "Health Promotion and Maintenance", null, (short)2, ExamTrack.PN, 12m, 6m },
                    { (short)15, "PSI", "Psychosocial Integrity", null, (short)3, ExamTrack.PN, 15m, 9m },
                    { (short)16, "PHI", "Physiological Integrity", null, (short)4, ExamTrack.PN, null, null },
                    { (short)2, "SECE.MOC", "Management of Care", (short)1, (short)1, ExamTrack.RN, 21m, 15m },
                    { (short)3, "SECE.SIC", "Safety and Infection Control", (short)1, (short)2, ExamTrack.RN, 16m, 10m },
                    { (short)7, "PHI.BCC", "Basic Care and Comfort", (short)6, (short)1, ExamTrack.RN, 12m, 6m },
                    { (short)8, "PHI.PPT", "Pharmacological and Parenteral Therapies", (short)6, (short)2, ExamTrack.RN, 19m, 13m },
                    { (short)9, "PHI.RRP", "Reduction of Risk Potential", (short)6, (short)3, ExamTrack.RN, 15m, 9m },
                    { (short)10, "PHI.PA", "Physiological Adaptation", (short)6, (short)4, ExamTrack.RN, 17m, 11m },
                    { (short)12, "SECE.CC", "Coordinated Care", (short)11, (short)1, ExamTrack.PN, 24m, 18m },
                    { (short)13, "SECE.SIC", "Safety and Infection Control", (short)11, (short)2, ExamTrack.PN, 16m, 10m },
                    { (short)17, "PHI.BCC", "Basic Care and Comfort", (short)16, (short)1, ExamTrack.PN, 13m, 7m },
                    { (short)18, "PHI.PT", "Pharmacological Therapies", (short)16, (short)2, ExamTrack.PN, 16m, 10m },
                    { (short)19, "PHI.RRP", "Reduction of Risk Potential", (short)16, (short)3, ExamTrack.PN, 15m, 9m },
                    { (short)20, "PHI.PA", "Physiological Adaptation", (short)16, (short)4, ExamTrack.PN, 13m, 7m }
                });

            migrationBuilder.CreateIndex(
                name: "ix_body_systems_slug",
                table: "body_systems",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_client_needs_categories_parent_id",
                table: "client_needs_categories",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "ix_client_needs_categories_track_code",
                table: "client_needs_categories",
                columns: new[] { "track", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_media_assets_created_by",
                table: "media_assets",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "ix_media_assets_storage_key",
                table: "media_assets",
                column: "storage_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_question_bank_items_question_id",
                table: "question_bank_items",
                column: "question_id");

            migrationBuilder.CreateIndex(
                name: "ix_question_versions_author_id",
                table: "question_versions",
                column: "author_id");

            migrationBuilder.CreateIndex(
                name: "ix_question_versions_body_system_id",
                table: "question_versions",
                column: "body_system_id");

            migrationBuilder.CreateIndex(
                name: "ix_question_versions_case_study_id",
                table: "question_versions",
                column: "case_study_id");

            migrationBuilder.CreateIndex(
                name: "ix_question_versions_question_id_version",
                table: "question_versions",
                columns: new[] { "question_id", "version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_question_versions_reviewer_id",
                table: "question_versions",
                column: "reviewer_id");

            migrationBuilder.CreateIndex(
                name: "qv_filter_idx",
                table: "question_versions",
                columns: new[] { "client_needs_id", "body_system_id", "item_type", "difficulty" },
                filter: "status = 'published'");

            migrationBuilder.CreateIndex(
                name: "qv_question_idx",
                table: "question_versions",
                columns: new[] { "question_id", "version" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "qv_review_queue_idx",
                table: "question_versions",
                columns: new[] { "status", "created_at" },
                filter: "status = 'in_review'");

            migrationBuilder.CreateIndex(
                name: "ix_questions_created_by",
                table: "questions",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "ix_questions_current_version_id",
                table: "questions",
                column: "current_version_id");

            migrationBuilder.AddForeignKey(
                name: "fk_question_bank_items_questions_question_id",
                table: "question_bank_items",
                column: "question_id",
                principalTable: "questions",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_question_versions_questions_question_id",
                table: "question_versions",
                column: "question_id",
                principalTable: "questions",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_question_versions_questions_question_id",
                table: "question_versions");

            migrationBuilder.DropTable(
                name: "media_assets");

            migrationBuilder.DropTable(
                name: "question_bank_items");

            migrationBuilder.DropTable(
                name: "question_banks");

            migrationBuilder.DropTable(
                name: "questions");

            migrationBuilder.DropTable(
                name: "question_versions");

            migrationBuilder.DropTable(
                name: "body_systems");

            migrationBuilder.DropTable(
                name: "case_studies");

            migrationBuilder.DropTable(
                name: "client_needs_categories");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:email_token_purpose", "change_email,reset_password,verify_email")
                .Annotation("Npgsql:Enum:exam_track", "PN,RN")
                .Annotation("Npgsql:Enum:plan_tier", "free,premium")
                .Annotation("Npgsql:Enum:user_role", "admin,author,candidate,educator,reviewer")
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
        }
    }
}
