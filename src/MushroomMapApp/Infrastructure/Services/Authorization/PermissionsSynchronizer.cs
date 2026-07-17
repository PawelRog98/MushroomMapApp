using Microsoft.EntityFrameworkCore;
using MushroomMapApp.Domain.Data;
using MushroomMapApp.Domain.Entities;
using MushroomMapApp.Domain.Interfaces;

namespace MushroomMapApp.Infrastructure.Services.Authorization;

public class PermissionsSynchronizer : IPermissionsSynchronizer
{
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

        foreach (var definition in definedPermissions)
        {
            if (existingPermissions.TryGetValue(definition.Code, out var existingPermission))
            {
                existingPermission.Name = definition.Name;
                existingPermission.IsActive = true;
            }
            else
            {
                _context.Permissions.AddAsync(new Permission
                {
                    Code = definition.Code,
                    Name = definition.Name,
                    IsActive = true
                },
                cancellationToken);
            }

            foreach (var existing in existingPermissions.Values)
            {
                var stillExists = definedPermissions.Any(x=>x.Code == existing.Code);
                if(!stillExists)
                    existing.IsActive = false;
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
