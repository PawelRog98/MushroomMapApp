using MushroomMapApp.Domain.Interfaces;

namespace MushroomMapApp.Features.Locations.GetLocations;

public class LocationsPermissionContextFactory : IPermissionsContextFactory<LocationPermissionContext>
{
    private readonly ICurrentUser _currentUser;
    private readonly IPermissionService _permissionService;

    public LocationsPermissionContextFactory(IPermissionService permissionService, ICurrentUser currentUser)
    {
        _permissionService = permissionService;
        _currentUser = currentUser;
    }

    public async Task<LocationPermissionContext> Create(CancellationToken cancellationToken)
    {
        var permissions = await _permissionService.GetPermissions(_currentUser.UserId.Value, cancellationToken);

        return new LocationPermissionContext
        {
            UserId = _currentUser.UserId.Value,
            Permissions = permissions
        };
    }
}
