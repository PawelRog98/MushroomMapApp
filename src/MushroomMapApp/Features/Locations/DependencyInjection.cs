using MushroomMapApp.Domain.Entities;
using MushroomMapApp.Domain.Interfaces;
using MushroomMapApp.Features.Locations.GetLocations;

namespace MushroomMapApp.Features.Locations;

public static class DependencyInjection
{
    public static IServiceCollection AddLocationsFeature(this IServiceCollection services)
    {
        services.AddScoped<IResourcePermissionEvaluator< Location, LocationPermissionContext, LocationPermissionResult>, LocationPermissionEvaluator>();

        services.AddScoped<
            IPermissionsContextFactory<LocationPermissionContext>,
            LocationsPermissionContextFactory>();

        return services;
    }
}
