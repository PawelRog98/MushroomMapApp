using MediatR;
using Microsoft.EntityFrameworkCore;
using MushroomMapApp.Domain.Data;
using MushroomMapApp.Domain.Enums;
using MushroomMapApp.Domain.Interfaces;
using MushroomMapApp.Features.Common.Pagination;

namespace MushroomMapApp.Features.Users.GetAllUsers;

public record GetAllUsersQuery(PaginationRequest Paging, SortRequest Sorting) : IRequest<PagedResult<UserListItemDto>>;

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, PagedResult<UserListItemDto>>
{
    private readonly AppDbContext _context;
    private readonly IPermissionService _permissionService;

    public GetAllUsersQueryHandler(AppDbContext context, IPermissionService permissionService)
    {
        _context = context;
        _permissionService = permissionService;
    }

    public async Task<PagedResult<UserListItemDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var users = _context.Users
            .AsNoTracking()
            .Include(x => x.Suspensions)
            .Include(x => x.Role);

        var paged = await users.PaginateList(request.Paging,
            q => q.SortUsers(request.Sorting),
            cancellationToken);

        var usersDto = paged.items
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

        var usersByPublicId = paged.items.ToDictionary(u => u.PublicId);

        foreach (var user in usersDto)
        {
            var permissions = await _permissionService.GetPermissions(usersByPublicId[user.PublicId].Id, cancellationToken);
            user.ActivePermissions = permissions.ToList();
        }

        return new PagedResult<UserListItemDto>(usersDto, paged.TotalCount, paged.Page, paged.PageSize);
    }
}
