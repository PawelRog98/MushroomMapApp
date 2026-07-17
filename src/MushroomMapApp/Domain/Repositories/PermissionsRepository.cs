using Microsoft.EntityFrameworkCore;
using MushroomMapApp.Domain.Data;
using MushroomMapApp.Domain.Interfaces;

namespace MushroomMapApp.Domain.Repositories;

public class PermissionsRepository : IPermissionsRepository
{
    private readonly AppDbContext _context;
    public PermissionsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<string>> GetAllPermissionsForUser(long userId,
        CancellationToken cancellationToken)
    {
        var rolePermissions = _context.UserRoles
            .Where(x => x.UserId == userId)
            .SelectMany(x => x.Role.Permissions)
            .Select(x => x.Permission.Code);

        var userPermissions = _context.UserPermissions
            .Where(x=>x.UserId == userId)
            .Select(x=>x.Permission.Code);

        return await rolePermissions.Union(userPermissions)
            .Distinct()
            .ToListAsync(cancellationToken);
    }
}
