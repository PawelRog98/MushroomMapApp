using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MushroomMapApp.Domain.Entities.Configurations;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).IsRequired().HasMaxLength(256);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(256);
        builder.Property(x => x.IsActive).IsRequired();
        builder.HasIndex(x => x.PublicId).IsUnique();
    }
}
