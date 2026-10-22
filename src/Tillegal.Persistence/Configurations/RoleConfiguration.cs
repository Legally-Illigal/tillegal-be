using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tillegal.Persistence.DataModels;

namespace Tillegal.Persistence.Configurations;

public sealed class RoleConfiguration : IEntityTypeConfiguration<RoleDataModel>
{
    public void Configure(EntityTypeBuilder<RoleDataModel> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .HasColumnType("text");
    }
}
