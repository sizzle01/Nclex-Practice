using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nclex.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class F27_TestSetTopic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "topic_section",
                table: "test_sets",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "topic_section",
                table: "test_sets");
        }
    }
}
