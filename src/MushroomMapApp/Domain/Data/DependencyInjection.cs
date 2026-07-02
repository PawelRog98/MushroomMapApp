using Microsoft.EntityFrameworkCore;

namespace MushroomMapApp.Domain.Data;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString, o => o.UseNetTopologySuite()));

        services.AddScoped<DbSeeder>();

        return services;
    }
}
