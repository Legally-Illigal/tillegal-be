using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tillegal.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ChangeAiEvaluationToConversation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AiEvaluationLogs_ConversationMessages_MessageId",
                table: "AiEvaluationLogs");

            migrationBuilder.RenameColumn(
                name: "MessageId",
                table: "AiEvaluationLogs",
                newName: "ConversationId");

            migrationBuilder.RenameIndex(
                name: "IX_AiEvaluationLogs_MessageId",
                table: "AiEvaluationLogs",
                newName: "IX_AiEvaluationLogs_ConversationId");

            migrationBuilder.Sql(@"
                UPDATE ""AiEvaluationLogs"" a
                SET ""ConversationId"" = m.""ConversationId""
                FROM ""ConversationMessages"" m
                WHERE a.""ConversationId"" = m.""Id"";

                DELETE FROM ""AiEvaluationLogs""
                WHERE ""ConversationId"" NOT IN (SELECT ""Id"" FROM ""Conversations"");
            ");

            migrationBuilder.AddForeignKey(
                name: "FK_AiEvaluationLogs_Conversations_ConversationId",
                table: "AiEvaluationLogs",
                column: "ConversationId",
                principalTable: "Conversations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AiEvaluationLogs_Conversations_ConversationId",
                table: "AiEvaluationLogs");

            migrationBuilder.RenameColumn(
                name: "ConversationId",
                table: "AiEvaluationLogs",
                newName: "MessageId");

            migrationBuilder.RenameIndex(
                name: "IX_AiEvaluationLogs_ConversationId",
                table: "AiEvaluationLogs",
                newName: "IX_AiEvaluationLogs_MessageId");

            migrationBuilder.Sql(@"
                UPDATE ""AiEvaluationLogs"" a
                SET ""MessageId"" = m.""Id""
                FROM (
                    SELECT DISTINCT ON (""ConversationId"") ""Id"", ""ConversationId""
                    FROM ""ConversationMessages""
                    ORDER BY ""ConversationId"", ""CreatedAt"" ASC
                ) m
                WHERE a.""MessageId"" = m.""ConversationId"";

                DELETE FROM ""AiEvaluationLogs""
                WHERE ""MessageId"" NOT IN (SELECT ""Id"" FROM ""ConversationMessages"");
            ");

            migrationBuilder.AddForeignKey(
                name: "FK_AiEvaluationLogs_ConversationMessages_MessageId",
                table: "AiEvaluationLogs",
                column: "MessageId",
                principalTable: "ConversationMessages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

