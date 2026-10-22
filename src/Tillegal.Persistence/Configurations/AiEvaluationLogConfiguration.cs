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

        builder.Property(e => e.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(e => e.UpdatedAt)
            .HasColumnType("timestamp with time zone");

        builder.Property(e => e.DeletedAt)
            .HasColumnType("timestamp with time zone");

        builder.Property(e => e.MessageId)
            .IsRequired();

        builder.Property(e => e.IsHelpful)
            .HasColumnType("boolean");

        builder.Property(e => e.Rating)
            .HasColumnType("integer");

        builder.Property(e => e.FeedbackComment)
            .HasColumnType("text");

        // Indexes
        builder.HasIndex(e => e.CreatedAt)
            .HasDatabaseName("IX_AiEvaluationLogs_CreatedAt");

        builder.HasIndex(e => e.EvaluatorUserId)
            .HasDatabaseName("IX_AiEvaluationLogs_EvaluatorUserId");

        builder.HasIndex(e => e.MessageId)
            .HasDatabaseName("IX_AiEvaluationLogs_MessageId");

        // Foreign keys
        builder.HasOne(e => e.EvaluatorUser)
            .WithMany(u => u.AiEvaluationLogs)
            .HasForeignKey(e => e.EvaluatorUserId)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("FK_AiEvaluationLogs_Users_EvaluatorUserId");

        builder.HasOne(e => e.Message)
            .WithMany(m => m.AiEvaluationLogs)
            .HasForeignKey(e => e.MessageId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_AiEvaluationLogs_ConversationMessages_MessageId");
    }
}
