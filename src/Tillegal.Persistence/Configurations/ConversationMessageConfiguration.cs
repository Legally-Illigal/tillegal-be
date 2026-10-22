using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tillegal.Persistence.DataModels;

namespace Tillegal.Persistence.Configurations;

public sealed class ConversationMessageConfiguration : IEntityTypeConfiguration<ConversationMessageDataModel>
{
    public void Configure(EntityTypeBuilder<ConversationMessageDataModel> builder)
    {
        builder.ToTable("ConversationMessages");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(m => m.UpdatedAt)
            .HasColumnType("timestamp with time zone");

        builder.Property(m => m.DeletedAt)
            .HasColumnType("timestamp with time zone");

        builder.Property(m => m.ConversationId)
            .IsRequired();

        builder.Property(m => m.Role)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(m => m.Content)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(m => m.Citations)
            .HasColumnType("jsonb");

        // Indexes
        builder.HasIndex(m => m.ConversationId)
            .HasDatabaseName("IX_ConversationMessages_ConversationId");

        builder.HasIndex(m => m.CreatedAt)
            .HasDatabaseName("IX_ConversationMessages_CreatedAt");

        // Foreign keys
        builder.HasOne(m => m.Conversation)
            .WithMany(c => c.Messages)
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_ConversationMessages_Conversations_ConversationId");
    }
}
