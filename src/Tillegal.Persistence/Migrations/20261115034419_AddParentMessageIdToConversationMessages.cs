using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tillegal.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddParentMessageIdToConversationMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ParentMessageId",
                table: "ConversationMessages",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConversationMessages_ParentMessageId",
                table: "ConversationMessages",
                column: "ParentMessageId");

            migrationBuilder.AddForeignKey(
                name: "FK_ConversationMessages_ConversationMessages_ParentMessageId",
                table: "ConversationMessages",
                column: "ParentMessageId",
                principalTable: "ConversationMessages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConversationMessages_ConversationMessages_ParentMessageId",
                table: "ConversationMessages");

            migrationBuilder.DropIndex(
                name: "IX_ConversationMessages_ParentMessageId",
                table: "ConversationMessages");

            migrationBuilder.DropColumn(
                name: "ParentMessageId",
                table: "ConversationMessages");
        }
    }
}
