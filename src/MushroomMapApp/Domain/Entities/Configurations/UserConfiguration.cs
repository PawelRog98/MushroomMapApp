using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MushroomMapApp.Domain.Entities;

namespace MushroomMapApp.Domain.Entities.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Email).IsRequired().HasMaxLength(256);
        builder.HasIndex(e => e.Email).IsUnique();
        builder.Property(e => e.PublicNick).IsRequired().HasMaxLength(100);
        builder.HasOne(e => e.Role).WithMany().HasForeignKey(e => e.RoleId);
    }
}
