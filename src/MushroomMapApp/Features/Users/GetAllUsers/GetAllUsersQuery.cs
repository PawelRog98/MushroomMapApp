using MediatR;
using Microsoft.EntityFrameworkCore;
using MushroomMapApp.Domain.Data;
using MushroomMapApp.Domain.Enums;
using MushroomMapApp.Domain.Interfaces;

namespace MushroomMapApp.Features.Users.GetAllUsers;

public record GetAllUsersQuery : IRequest<List<UserListItemDto>>;

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, List<UserListItemDto>>
{
    private readonly AppDbContext _context;
    private readonly IPermissionService _permissionService;

    public GetAllUsersQueryHandler(AppDbContext context, IPermissionService permissionService)
    {
        _context = context;
        _permissionService = permissionService;
    }

    public async Task<List<UserListItemDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var users = await _context.Users
                .Include(x => x.Suspensions)
                .Include(x => x.Role)
                .ToListAsync(cancellationToken);

            var usersDto = users
                .Select(u => new UserListItemDto
                {
                    PublicId = u.PublicId,
                    PublicNick = u.PublicNick,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    RoleName = u.Role.Name,
                    IsActiveSuspension = u.Suspensions.Any(s => s.Status == SuspensionStatusEnum.Active),
                    SuspensionEndDate = u.Suspensions
                        .Where(s => s.Status == SuspensionStatusEnum.Active)
                        .Select(s => s.EndDate)
                        .FirstOrDefault()
                })
                .ToList();

            foreach (var user in usersDto)
            {
                var currentUser = users.FirstOrDefault(u => u.PublicId == user.PublicId);
                var permissions = await _permissionService.GetPermissions(currentUser.Id, cancellationToken);
                user.ActivePermissions = permissions.ToList();
            }

            return usersDto;
        }
        catch (Exception ex)
        {
            throw;
        }
    }
}
