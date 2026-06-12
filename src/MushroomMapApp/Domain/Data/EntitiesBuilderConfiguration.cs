using Microsoft.EntityFrameworkCore;
using MushroomMapApp.Domain.Abstractions;

namespace MushroomMapApp.Domain.Data;

public class EntitiesBuilderConfiguration
{
    public void Configure(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;

            if (typeof(ICommonData).IsAssignableFrom(clrType))
            {
                modelBuilder.Entity(clrType)
                    .Property("PublicId")
                    .HasDefaultValueSql("gen_random_uuid()");
            }
        }
    }
}