using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nclex.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class F21_RenameSafetyCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "client_needs_categories",
                keyColumn: "id",
                keyValue: (short)3,
                column: "name",
                value: "Safety and Infection Prevention and Control");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "client_needs_categories",
                keyColumn: "id",
                keyValue: (short)3,
                column: "name",
                value: "Safety and Infection Control");
        }
    }
}
