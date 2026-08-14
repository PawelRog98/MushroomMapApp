using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MushroomMapApp.Domain.Entities;

namespace MushroomMapApp.Domain.Entities.Configurations;

public class SuspensionConfiguration : IEntityTypeConfiguration<Suspension>
{
    public void Configure(EntityTypeBuilder<Suspension> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Reason)
            .IsRequired()
            .HasMaxLength(1024);

        builder.HasOne(e => e.User)
            .WithMany(u => u.Suspensions)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.SuspendedBy)
            .WithMany()
            .HasForeignKey(e => e.SuspendedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.PublicId)
            .IsUnique();
    }
}
