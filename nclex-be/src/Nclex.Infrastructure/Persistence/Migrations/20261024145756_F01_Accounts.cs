using System;
using System.Net;
using Microsoft.EntityFrameworkCore.Migrations;
using Nclex.Domain.Entities;

#nullable disable

namespace Nclex.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class F01_Accounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Hand-edited: EF lists enum labels alphabetically; these follow the DDL order in
            // database.md section 2.1, which is the order Postgres uses to sort enum values.
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:exam_track", "RN,PN")
                .Annotation("Npgsql:Enum:plan_tier", "free,premium")
                .Annotation("Npgsql:Enum:user_role", "candidate,educator,author,reviewer,admin")
                .Annotation("Npgsql:PostgresExtension:citext", ",,")
                .Annotation("Npgsql:PostgresExtension:pgcrypto", ",,")
                .OldAnnotation("Npgsql:PostgresExtension:citext", ",,")
                .OldAnnotation("Npgsql:PostgresExtension:pgcrypto", ",,");

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    email = table.Column<string>(type: "citext", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: true),
                    email_verified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    role = table.Column<UserRole>(type: "user_role", nullable: false, defaultValue: UserRole.Candidate),
                    track = table.Column<ExamTrack>(type: "exam_track", nullable: false),
                    plan_tier = table.Column<PlanTier>(type: "plan_tier", nullable: false, defaultValue: PlanTier.Free),
                    display_name = table.Column<string>(type: "text", nullable: true),
                    timezone = table.Column<string>(type: "text", nullable: false, defaultValue: "UTC"),
                    target_exam_date = table.Column<DateOnly>(type: "date", nullable: true),
                    locked_until = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    password_changed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "refresh_sessions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    family_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token_hash = table.Column<byte[]>(type: "bytea", nullable: false),
                    user_agent = table.Column<string>(type: "text", nullable: true),
                    ip = table.Column<IPAddress>(type: "inet", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    last_used_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    revoked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    replaced_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_refresh_sessions", x => x.id);
                    table.ForeignKey(
                        name: "fk_refresh_sessions_refresh_sessions_replaced_by",
                        column: x => x.replaced_by,
                        principalTable: "refresh_sessions",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_refresh_sessions_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_refresh_sessions_family_id",
                table: "refresh_sessions",
                column: "family_id");

            migrationBuilder.CreateIndex(
                name: "ix_refresh_sessions_replaced_by",
                table: "refresh_sessions",
                column: "replaced_by");

            migrationBuilder.CreateIndex(
                name: "ix_refresh_sessions_token_hash",
                table: "refresh_sessions",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_refresh_sessions_user_id",
                table: "refresh_sessions",
                column: "user_id",
                filter: "revoked_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_users_email",
                table: "users",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "refresh_sessions");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:citext", ",,")
                .Annotation("Npgsql:PostgresExtension:pgcrypto", ",,")
                .OldAnnotation("Npgsql:Enum:exam_track", "PN,RN")
                .OldAnnotation("Npgsql:Enum:plan_tier", "free,premium")
                .OldAnnotation("Npgsql:Enum:user_role", "admin,author,candidate,educator,reviewer")
                .OldAnnotation("Npgsql:PostgresExtension:citext", ",,")
                .OldAnnotation("Npgsql:PostgresExtension:pgcrypto", ",,");
        }
    }
}
