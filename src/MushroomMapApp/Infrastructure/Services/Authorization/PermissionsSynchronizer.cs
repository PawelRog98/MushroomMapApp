using Microsoft.EntityFrameworkCore;
using MushroomMapApp.Domain.Data;
using MushroomMapApp.Domain.Entities;
using MushroomMapApp.Domain.Interfaces;

namespace MushroomMapApp.Infrastructure.Services.Authorization;

public class PermissionsSynchronizer : IPermissionsSynchronizer
{
    private const string AdministratorRoleName = "Administrator";

    private readonly AppDbContext _context;
    private readonly IPermissionRegistry _permissionRegistry;

    public PermissionsSynchronizer(AppDbContext context, IPermissionRegistry permissionRegistry)
    {
        _context = context;
        _permissionRegistry = permissionRegistry;
    }

    public async Task Synchronize(CancellationToken cancellationToken)
    {
        var definedPermissions = _permissionRegistry.GetAll();

        var existingPermissions = await _context.Permissions
            .ToDictionaryAsync(x => x.Code, cancellationToken);

        var hasChanges = false;

        foreach (var definition in definedPermissions)
        {
            if (existingPermissions.TryGetValue(definition.Code, out var existingPermission))
            {
                if (existingPermission.Name != definition.Name || !existingPermission.IsActive)
                {
                    existingPermission.Name = definition.Name;
                    existingPermission.IsActive = true;
                    hasChanges = true;
                }
            }
            else
            {
                _context.Permissions.Add(new Permission
                {
                    Code = definition.Code,
                    Name = definition.Name,
                    IsActive = true
                });
                hasChanges = true;
            }
        }

        foreach (var existing in existingPermissions.Values)
        {
            var stillExists = definedPermissions.Any(x => x.Code == existing.Code);
            if (!stillExists && existing.IsActive)
            {
                existing.IsActive = false;
                hasChanges = true;
            }
        }

        if (hasChanges)
            await _context.SaveChangesAsync(cancellationToken);

        await GrantAllPermissionsToAdministratorAsync(cancellationToken);
    }

    private async Task GrantAllPermissionsToAdministratorAsync(CancellationToken cancellationToken)
    {
        var adminRole = await _context.Roles
            .FirstOrDefaultAsync(x => x.Name == AdministratorRoleName, cancellationToken);

        if (adminRole == null)
            return;

        var grantedPermissionIds = await _context.RolePermissions
            .Where(rp => rp.RoleId == adminRole.Id)
            .Select(rp => rp.PermissionId)
            .ToListAsync(cancellationToken);

        var activePermissionIds = await _context.Permissions
            .Where(x => x.IsActive)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var missingPermissionIds = activePermissionIds
            .Except(grantedPermissionIds)
            .ToList();

        if (missingPermissionIds.Count == 0)
            return;

        _context.RolePermissions.AddRange(
            missingPermissionIds.Select(permissionId => new RolePermission
            {
                RoleId = adminRole.Id,
                PermissionId = permissionId
            }));

        await _context.SaveChangesAsync(cancellationToken);
    }
}
