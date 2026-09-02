using MediatR;
using MushroomMapApp.Domain.Data;
using MushroomMapApp.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using MushroomMapApp.Domain.Entities;
using MushroomMapApp.Domain.Interfaces;

namespace MushroomMapApp.Features.Users.AttachPermissions;
public record AttachPermissionsRequest(Guid UserPublicId, List<string> Permissions);

public record AttachPermissionsCommand(long userId, AttachPermissionsRequest Request) : IRequest<Unit>;
public class AttachPermissionsCommandHandler : IRequestHandler<AttachPermissionsCommand, Unit>
{
    private readonly AppDbContext _context;
    private readonly IPermissionService _permissionService;
    public AttachPermissionsCommandHandler(AppDbContext context,  IPermissionService permissionService)
    {
        _context = context;
        _permissionService = permissionService;
    }

    public async Task<Unit> Handle(AttachPermissionsCommand request, CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(x=>x.PublicId == request.Request.UserPublicId, cancellationToken);

            if (user == null)
                throw new BadRequestException("Invalid user data.");

            var permissionCodes = request.Request.Permissions.Distinct().ToList();

            var existing = await _context.UserPermissions
                .Where(x => x.UserId == user.Id)
                .ToListAsync(cancellationToken);

            var permissions = await _context.Permissions
                .Where(x => permissionCodes.Contains(x.Code))
                .ToListAsync(cancellationToken);

            var permissionIds = permissions.Select(x => x.Id).ToList();
            var userPermissionsToDelete = existing.Where(x => !permissionIds.Contains(x.PermissionId));

            _context.UserPermissions.RemoveRange(userPermissionsToDelete);

            foreach (var permission in permissions.Where(x => existing.All(y => y.PermissionId != x.Id)))
            {
                await _context.UserPermissions.AddAsync(new UserPermission
                {
                    UserId = user.Id,
                    PermissionId = permission.Id
                });
            }

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Unit.Value;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
