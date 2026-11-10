using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tillegal.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFileHashToUserAttachment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FileHash",
                table: "UserAttachments",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserAttachments_FileHash",
                table: "UserAttachments",
                column: "FileHash");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserAttachments_FileHash",
                table: "UserAttachments");

            migrationBuilder.DropColumn(
                name: "FileHash",
                table: "UserAttachments");
        }
    }
}
