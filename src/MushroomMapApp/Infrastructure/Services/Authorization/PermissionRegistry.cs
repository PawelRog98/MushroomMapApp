using System.Reflection;
using MushroomMapApp.Domain.Interfaces;
using MushroomMapApp.Domain.Permissions;

namespace MushroomMapApp.Infrastructure.Services.Authorization;

public class PermissionRegistry :  IPermissionRegistry
{
    private readonly Dictionary<string, PermissionDefinition> _permissions;
    private readonly Dictionary<string, IReadOnlySet<string>> _expanded;
    public PermissionRegistry()
    {
        _permissions = GetPermissions();
        ValidateDefinitions();
        _expanded = BuildExpandedPermissions();
    }

    public IEnumerable<PermissionDefinition> GetAll()
        => _permissions.Values;

    public IReadOnlySet<string> GetExpanded(string permission)
    {
        if(!_expanded.TryGetValue(permission, out var expanded))
            throw new InvalidOperationException($"Permission '{permission}' does not exist.");

        return expanded;
    }

    private static Dictionary<string, PermissionDefinition> GetPermissions()
    {
        var result = new Dictionary<string, PermissionDefinition>();

        var assembly = typeof(PermissionDefinition).Assembly;

        foreach (var type in assembly.GetTypes())
        {
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                if(field.FieldType != typeof(PermissionDefinition))
                    continue;

                var permission = (PermissionDefinition)field.GetValue(null);
                result.Add(permission.Code, permission);
            }
        }

        return result;
    }

    private Dictionary<string, IReadOnlySet<string>> BuildExpandedPermissions()
    {
        var result = new Dictionary<string, IReadOnlySet<string>>();

        foreach (var permission in _permissions.Values)
        {
            var expanded = Expand(permission.Code, new HashSet<string>());
            result.Add(permission.Code, expanded);
        }

        return result;
    }

    private IReadOnlySet<string> Expand(string permissionCode, HashSet<string> visited)
    {
        if (!visited.Add(permissionCode))
            return visited;

        if (!_permissions.TryGetValue(permissionCode, out var permission))
            throw new InvalidOperationException($"Permission {permission} not found");

        foreach (var implied in permission.Implies)
        {
            Expand(implied, visited);
        }

        return visited;
    }

    private void ValidateDefinitions()
    {
        foreach (var definition in _permissions.Values)
        {
            foreach (var implied in definition.Implies)
            {
                if (!_permissions.ContainsKey(implied))
                {
                    throw new InvalidOperationException($"Permission '{definition.Code}' implies unknown permission '{implied}'.");
                }
            }
        }
    }
}
