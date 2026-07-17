using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MushroomMapApp.Domain.Entities.Configurations;

public class ReactionTypeConfiguration : IEntityTypeConfiguration<ReactionType>
{
    public void Configure(EntityTypeBuilder<ReactionType> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Key).IsRequired().HasMaxLength(128);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(128);
        builder.Property(x => x.Icon).IsRequired().HasMaxLength(128);
        builder.HasIndex(x => x.PublicId).IsUnique();
    }
}
