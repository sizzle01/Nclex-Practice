using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Nclex.Domain.Entities;

#nullable disable

namespace Nclex.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class F13_PerformanceSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "performance_snapshots",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    track = table.Column<ExamTrack>(type: "exam_track", nullable: false),
                    dimension = table.Column<string>(type: "text", nullable: false),
                    dimension_key = table.Column<string>(type: "text", nullable: false),
                    items_answered = table.Column<int>(type: "integer", nullable: false),
                    items_correct = table.Column<int>(type: "integer", nullable: false),
                    points_earned = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    max_points = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    accuracy = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: false, computedColumnSql: "CASE WHEN max_points > 0 THEN points_earned / max_points ELSE 0 END", stored: true),
                    avg_time_ms = table.Column<int>(type: "integer", nullable: false),
                    attempts_count = table.Column<int>(type: "integer", nullable: false),
                    last_attempt_id = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_performance_snapshots", x => new { x.user_id, x.track, x.dimension, x.dimension_key });
                    table.ForeignKey(
                        name: "fk_performance_snapshots_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "performance_snapshots");
        }
    }
}
