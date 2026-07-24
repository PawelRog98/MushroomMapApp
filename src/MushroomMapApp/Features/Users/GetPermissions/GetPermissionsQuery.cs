using MediatR;
using MushroomMapApp.Domain.Interfaces;

namespace MushroomMapApp.Features.Users.GetPermissions;

public record GetPermissionsQuery(long UserId) : IRequest<UserPermissionsDto>;

public class GetPermissionsQueryHandler : IRequestHandler<GetPermissionsQuery, UserPermissionsDto>
{
    private readonly IPermissionService _permissionService;

    public GetPermissionsQueryHandler(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    public async Task<UserPermissionsDto> Handle(GetPermissionsQuery query, CancellationToken cancellationToken)
    {
        var permissions = await _permissionService.GetPermissions(query.UserId, cancellationToken);
        return new UserPermissionsDto
        {
            Permissions = permissions.ToArray()
        };
    }
}
