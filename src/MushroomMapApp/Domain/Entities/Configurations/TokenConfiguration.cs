using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MushroomMapApp.Domain.Entities;

namespace MushroomMapApp.Domain.Entities.Configurations;

public class TokenConfiguration : IEntityTypeConfiguration<Token>
{
    public void Configure(EntityTypeBuilder<Token> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.TokenData).IsRequired().HasMaxLength(500);
        builder.HasOne(e => e.User).WithMany(u => u.Tokens).HasForeignKey(e => e.UserId);
    }
}
