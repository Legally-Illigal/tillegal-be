using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tillegal.Persistence.DataModels;

namespace Tillegal.Persistence.Configurations;

public sealed class AiEvaluationLogConfiguration : IEntityTypeConfiguration<AiEvaluationLogDataModel>
{
    public void Configure(EntityTypeBuilder<AiEvaluationLogDataModel> builder)
    {
        builder.ToTable("AiEvaluationLogs");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.FeedbackComment)
            .HasColumnType("text");

        builder.HasIndex(e => e.MessageId)
            .HasDatabaseName("IX_AiEvaluationLogs_MessageId");

        builder.HasIndex(e => e.EvaluatorUserId)
            .HasDatabaseName("IX_AiEvaluationLogs_EvaluatorUserId");

        builder.HasIndex(e => e.CreatedAt)
            .HasDatabaseName("IX_AiEvaluationLogs_CreatedAt");

        builder.HasOne(e => e.Message)
            .WithMany(m => m.AiEvaluationLogs)
            .HasForeignKey(e => e.MessageId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_AiEvaluationLogs_ConversationMessages_MessageId");

        builder.HasOne(e => e.EvaluatorUser)
            .WithMany(u => u.AiEvaluationLogs)
            .HasForeignKey(e => e.EvaluatorUserId)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("FK_AiEvaluationLogs_Users_EvaluatorUserId");
    }
}

