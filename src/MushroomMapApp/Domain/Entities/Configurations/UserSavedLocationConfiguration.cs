using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MushroomMapApp.Domain.Entities.Configurations;

public class UserSavedLocationConfiguration : IEntityTypeConfiguration<UserSavedLocation>
{
    public void Configure(EntityTypeBuilder<UserSavedLocation> builder)
    {
        builder.HasKey(x=>x.Id);

        builder.HasIndex(x => x.PublicId)
            .IsUnique();

        builder.HasIndex(x => x.UserId);

        builder.Property(x=>x.LocationName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.Coordinates)
            .IsRequired()
            .HasColumnType("geometry(Point, 4326)");

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
