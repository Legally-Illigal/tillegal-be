using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tillegal.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUserNameFromUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserName",
                table: "Users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserName",
                table: "Users",
                type: "text",
                nullable: true);
        }
    }
}
