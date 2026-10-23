using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nclex.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class F00_Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:citext", ",,")
                .Annotation("Npgsql:PostgresExtension:pgcrypto", ",,");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
