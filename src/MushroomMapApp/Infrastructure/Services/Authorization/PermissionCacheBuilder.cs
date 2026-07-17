using MushroomMapApp.Domain.Interfaces;

namespace MushroomMapApp.Infrastructure.Services.Authorization;

public class PermissionCacheBuilder : IPermissionCacheBuilder
{
    private readonly IPermissionsRepository _permissionsRepository;
    private readonly IPermissionRegistry _permissionRegistry;

    public PermissionCacheBuilder(IPermissionsRepository permissionsRepository,  IPermissionRegistry permissionRegistry)
    {
        _permissionsRepository = permissionsRepository;
        _permissionRegistry = permissionRegistry;
    }

    public async Task<HashSet<string>> Build(long userId, CancellationToken cancellationToken)
    {
        var assigned = await _permissionsRepository.GetAllPermissionsForUser(userId, cancellationToken);

        var permissions = new HashSet<string>();
        foreach (var permission in assigned)
        {
            permissions.UnionWith(_permissionRegistry.GetExpanded(permission));
        }

        return permissions;
    }
}
