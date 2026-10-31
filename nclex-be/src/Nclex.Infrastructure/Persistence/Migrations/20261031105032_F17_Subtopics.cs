using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nclex.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class F17_Subtopics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "subtopics",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    client_needs_id = table.Column<short>(type: "smallint", nullable: false),
                    slug = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    sort_order = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_subtopics", x => x.id);
                    table.ForeignKey(
                        name: "fk_subtopics_client_needs_categories_client_needs_id",
                        column: x => x.client_needs_id,
                        principalTable: "client_needs_categories",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "question_subtopics",
                columns: table => new
                {
                    question_id = table.Column<Guid>(type: "uuid", nullable: false),
                    subtopic_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_question_subtopics", x => new { x.question_id, x.subtopic_id });
                    table.ForeignKey(
                        name: "fk_question_subtopics_questions_question_id",
                        column: x => x.question_id,
                        principalTable: "questions",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_question_subtopics_subtopics_subtopic_id",
                        column: x => x.subtopic_id,
                        principalTable: "subtopics",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_question_subtopics_subtopic_id",
                table: "question_subtopics",
                column: "subtopic_id");

            migrationBuilder.CreateIndex(
                name: "ix_subtopics_client_needs_id_slug",
                table: "subtopics",
                columns: new[] { "client_needs_id", "slug" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "question_subtopics");

            migrationBuilder.DropTable(
                name: "subtopics");
        }
    }
}
