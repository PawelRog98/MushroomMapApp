using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MushroomMapApp.Domain.Entities.Configurations;

public class FileResourceConfiguration : IEntityTypeConfiguration<FileResource>
{
    public void Configure(EntityTypeBuilder<FileResource> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FileName)
            .IsRequired()
            .HasMaxLength(1024);

        builder.Property(x => x.Description)
            .HasMaxLength(2048);

        builder.Property(x => x.ContentType)
            .HasMaxLength(256);

        builder.Property(x => x.Size)
            .IsRequired();

        builder.Property(x => x.Type)
            .IsRequired()
            .HasMaxLength(64);

        builder.HasOne<Location>()
            .WithMany(l => l.FileResources)
            .HasForeignKey(x => x.LocationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ParentFileResource)
            .WithMany(x => x.Variant)
            .HasForeignKey(x => x.ParentFileResourceId)
            .HasPrincipalKey(x => x.Id)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.PublicId)
            .IsUnique();
    }
}
