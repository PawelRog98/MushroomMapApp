using MushroomMapApp.Domain.Permissions;

namespace MushroomMapApp.Domain.Interfaces;

public interface IPermissionRegistry
{
    IEnumerable<PermissionDefinition> GetAll();
    IReadOnlySet<string> GetExpanded(string permission);
}
