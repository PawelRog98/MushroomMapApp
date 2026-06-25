using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MushroomMapApp.Domain.Entities;

namespace MushroomMapApp.Domain.Entities.Configurations;

public class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(x => x.Text)
            .IsRequired()
            .HasMaxLength(4096);

        builder.Property(x => x.Coordinates)
            .IsRequired()
            .HasColumnType("geometry(Point, 4326)");

        builder.HasOne(x => x.CreatedBy)
            .WithMany(u => u.Locations)
            .HasForeignKey(x => x.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.PublicId)
            .IsUnique();

        builder.HasIndex(x => x.Coordinates)
            .HasMethod("GIST");
    }
}
