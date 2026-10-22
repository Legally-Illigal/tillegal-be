using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tillegal.Persistence.DataModels;

namespace Tillegal.Persistence.Configurations;

public sealed class UserAttachmentConfiguration : IEntityTypeConfiguration<UserAttachmentDataModel>
{
    public void Configure(EntityTypeBuilder<UserAttachmentDataModel> builder)
    {
        builder.ToTable("UserAttachments");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(a => a.UpdatedAt)
            .HasColumnType("timestamp with time zone");

        builder.Property(a => a.DeletedAt)
            .HasColumnType("timestamp with time zone");

        builder.Property(a => a.ConversationId)
            .IsRequired();

        builder.Property(a => a.UserId)
            .IsRequired();

        builder.Property(a => a.FileName)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(a => a.StoragePath)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(a => a.FileSize)
            .HasColumnType("bigint")
            .IsRequired();

        builder.Property(a => a.ContentType)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(a => a.ProcessingStatus)
            .HasColumnType("text")
            .HasDefaultValue("Pending")
            .IsRequired();

        builder.Property(a => a.TokenCount)
            .HasColumnType("integer");

        // Indexes
        builder.HasIndex(a => a.ConversationId)
            .HasDatabaseName("IX_UserAttachments_ConversationId");

        builder.HasIndex(a => a.UserId)
            .HasDatabaseName("IX_UserAttachments_UserId");

        builder.HasIndex(a => a.ProcessingStatus)
            .HasDatabaseName("IX_UserAttachments_ProcessingStatus");

        // Foreign keys
        builder.HasOne(a => a.User)
            .WithMany(u => u.UserAttachments)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_UserAttachments_Users_UserId");

        builder.HasOne(a => a.Conversation)
            .WithMany(c => c.Attachments)
            .HasForeignKey(a => a.ConversationId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_UserAttachments_Conversations_ConversationId");
    }
}
