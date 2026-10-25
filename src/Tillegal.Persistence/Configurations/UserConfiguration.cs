using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tillegal.Persistence.DataModels;

namespace Tillegal.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<UserDataModel>
{
    public void Configure(EntityTypeBuilder<UserDataModel> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.RoleId)
            .IsRequired();

        builder.Property(u => u.FullName)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(u => u.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(u => u.UpdatedAt)
            .HasColumnType("timestamp with time zone");

        builder.Property(u => u.DeletedAt)
            .HasColumnType("timestamp with time zone");

        builder.Property(u => u.Email)
            .HasColumnType("text");

        builder.Property(u => u.GoogleSubject)
            .HasColumnType("text");

        builder.HasIndex(u => u.GoogleSubject)
            .IsUnique();

        builder.Property(u => u.PasswordHash)
            .HasColumnType("text");

        builder.Property(u => u.PhoneNumber)
            .HasColumnType("text");

        builder.HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_UserRoles_Users_UserId");
    }
}
