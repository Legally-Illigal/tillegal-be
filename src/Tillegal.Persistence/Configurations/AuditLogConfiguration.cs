using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tillegal.Persistence.DataModels;

namespace Tillegal.Persistence.Configurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLogDataModel>
{
    public void Configure(EntityTypeBuilder<AuditLogDataModel> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(a => a.UpdatedAt)
            .HasColumnType("timestamp with time zone");

        builder.Property(a => a.DeletedAt)
            .HasColumnType("timestamp with time zone");

        builder.Property(a => a.EventType)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(a => a.ActorRole)
            .HasColumnType("text");

        builder.Property(a => a.IpAddress)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(a => a.TargetType)
            .HasColumnType("text");

        builder.Property(a => a.Action)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(a => a.OldValues)
            .HasColumnType("jsonb");

        builder.Property(a => a.NewValues)
            .HasColumnType("jsonb");

        builder.Property(a => a.Status)
            .HasColumnType("text")
            .HasDefaultValue("SUCCESS")
            .IsRequired();

        builder.Property(a => a.Description)
            .HasColumnType("text");

        // Indexes
        builder.HasIndex(a => a.CreatedAt)
            .HasDatabaseName("IX_AuditLogs_CreatedAt");

        builder.HasIndex(a => a.EventType)
            .HasDatabaseName("IX_AuditLogs_EventType");

        builder.HasIndex(a => a.ActorUserId)
            .HasDatabaseName("IX_AuditLogs_ActorUserId");

        builder.HasIndex(a => a.TargetUserId)
            .HasDatabaseName("IX_AuditLogs_TargetUserId");

        builder.HasIndex(a => a.Action)
            .HasDatabaseName("IX_AuditLogs_Action");

        builder.HasIndex(a => a.Status)
            .HasDatabaseName("IX_AuditLogs_Status");

        // Foreign keys
        builder.HasOne(a => a.ActorUser)
            .WithMany(u => u.ActorAuditLogs)
            .HasForeignKey(a => a.ActorUserId)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("FK_AuditLogs_Users_ActorUserId");

        builder.HasOne(a => a.TargetUser)
            .WithMany(u => u.TargetAuditLogs)
            .HasForeignKey(a => a.TargetUserId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_AuditLogs_Users_TargetUserId");
    }
}
