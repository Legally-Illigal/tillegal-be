using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tillegal.Persistence.DataModels;

namespace Tillegal.Persistence.Configurations;

public sealed class ConversationConfiguration : IEntityTypeConfiguration<ConversationDataModel>
{
    public void Configure(EntityTypeBuilder<ConversationDataModel> builder)
    {
        builder.ToTable("Conversations");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .HasColumnType("timestamp with time zone");

        builder.Property(c => c.DeletedAt)
            .HasColumnType("timestamp with time zone");

        builder.Property(c => c.UserId)
            .IsRequired();

        builder.Property(c => c.Title)
            .HasColumnType("text")
            .IsRequired();

        // Indexes
        builder.HasIndex(c => c.CreatedAt)
            .HasDatabaseName("IX_Conversations_CreatedAt");

        builder.HasIndex(c => c.UserId)
            .HasDatabaseName("IX_Conversations_UserId");

        // Foreign keys
        builder.HasOne(c => c.User)
            .WithMany(u => u.Conversations)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_Conversations_Users_UserId");
    }
}
