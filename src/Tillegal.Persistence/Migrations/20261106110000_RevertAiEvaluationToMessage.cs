using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tillegal.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RevertAiEvaluationToMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1
                        FROM information_schema.columns
                        WHERE table_name = 'AiEvaluationLogs' AND column_name = 'ConversationId'
                    ) THEN
                        -- Drop old foreign key if exists
                        IF EXISTS (
                            SELECT 1
                            FROM information_schema.table_constraints
                            WHERE constraint_name = 'FK_AiEvaluationLogs_Conversations_ConversationId'
                        ) THEN
                            ALTER TABLE ""AiEvaluationLogs"" DROP CONSTRAINT ""FK_AiEvaluationLogs_Conversations_ConversationId"";
                        END IF;

                        -- Rename column
                        ALTER TABLE ""AiEvaluationLogs"" RENAME COLUMN ""ConversationId"" TO ""MessageId"";

                        -- Rename index if exists
                        IF EXISTS (
                            SELECT 1
                            FROM pg_indexes
                            WHERE indexname = 'IX_AiEvaluationLogs_ConversationId'
                        ) THEN
                            ALTER INDEX ""IX_AiEvaluationLogs_ConversationId"" RENAME TO ""IX_AiEvaluationLogs_MessageId"";
                        END IF;

                        -- Update MessageId from assistant messages in that conversation, or delete orphaned
                        UPDATE ""AiEvaluationLogs"" a
                        SET ""MessageId"" = m.""Id""
                        FROM (
                            SELECT DISTINCT ON (""ConversationId"") ""Id"", ""ConversationId""
                            FROM ""ConversationMessages""
                            WHERE ""Role"" = 'assistant'
                            ORDER BY ""ConversationId"", ""CreatedAt"" ASC
                        ) m
                        WHERE a.""MessageId"" = m.""ConversationId"";

                        DELETE FROM ""AiEvaluationLogs""
                        WHERE ""MessageId"" NOT IN (SELECT ""Id"" FROM ""ConversationMessages"");

                        -- Add new FK constraint if not exists
                        IF NOT EXISTS (
                            SELECT 1
                            FROM information_schema.table_constraints
                            WHERE constraint_name = 'FK_AiEvaluationLogs_ConversationMessages_MessageId'
                        ) THEN
                            ALTER TABLE ""AiEvaluationLogs""
                            ADD CONSTRAINT ""FK_AiEvaluationLogs_ConversationMessages_MessageId""
                            FOREIGN KEY (""MessageId"") REFERENCES ""ConversationMessages"" (""Id"") ON DELETE CASCADE;
                        END IF;
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddForeignKey(
                name: "FK_AiEvaluationLogs_Conversations_ConversationId",
                table: "AiEvaluationLogs",
                column: "ConversationId",
                principalTable: "Conversations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

