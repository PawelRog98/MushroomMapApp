namespace MushroomMapApp.Domain.Interfaces;

public interface IPermissionService
{
    Task<IReadOnlySet<string>> GetPermissions(long userId, CancellationToken cancellationToken);
    Task<bool> HasPermission(long userId, string permissionName, CancellationToken cancellationToken);
}
