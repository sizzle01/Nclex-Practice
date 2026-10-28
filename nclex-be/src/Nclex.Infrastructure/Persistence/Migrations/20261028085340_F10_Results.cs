using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nclex.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class F10_Results : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "attempt_category_scores",
                columns: table => new
                {
                    attempt_id = table.Column<Guid>(type: "uuid", nullable: false),
                    dimension = table.Column<string>(type: "text", nullable: false),
                    dimension_key = table.Column<string>(type: "text", nullable: false),
                    items = table.Column<int>(type: "integer", nullable: false),
                    items_correct = table.Column<int>(type: "integer", nullable: false),
                    points_earned = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: false),
                    max_points = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: false),
                    time_ms = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_attempt_category_scores", x => new { x.attempt_id, x.dimension, x.dimension_key });
                    table.CheckConstraint("attempt_category_scores_dimension_check", "dimension IN ('client_needs','client_needs_parent','body_system','item_type','cjmm_skill','difficulty')");
                    table.ForeignKey(
                        name: "fk_attempt_category_scores_attempts_attempt_id",
                        column: x => x.attempt_id,
                        principalTable: "attempts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "attempt_results",
                columns: table => new
                {
                    attempt_id = table.Column<Guid>(type: "uuid", nullable: false),
                    points_earned = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: false),
                    max_points = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: false),
                    score_pct = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: false),
                    items_correct = table.Column<int>(type: "integer", nullable: false),
                    items_answered = table.Column<int>(type: "integer", nullable: false),
                    items_total = table.Column<int>(type: "integer", nullable: false),
                    total_time_ms = table.Column<long>(type: "bigint", nullable: false),
                    avg_time_ms = table.Column<int>(type: "integer", nullable: false),
                    pass_threshold = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: true),
                    passed = table.Column<bool>(type: "boolean", nullable: true),
                    readiness_label = table.Column<string>(type: "text", nullable: true),
                    cat_theta = table.Column<decimal>(type: "numeric(6,3)", precision: 6, scale: 3, nullable: true),
                    cat_se = table.Column<decimal>(type: "numeric(6,3)", precision: 6, scale: 3, nullable: true),
                    graded_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_attempt_results", x => x.attempt_id);
                    table.ForeignKey(
                        name: "fk_attempt_results_attempts_attempt_id",
                        column: x => x.attempt_id,
                        principalTable: "attempts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "attempt_category_scores");

            migrationBuilder.DropTable(
                name: "attempt_results");
        }
    }
}
