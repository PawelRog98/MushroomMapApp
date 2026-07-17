using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using MushroomMapApp.Domain.Interfaces;

namespace MushroomMapApp.Infrastructure.Services.Authorization;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IPermissionService _permissionService;

    public PermissionAuthorizationHandler(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim is null || !long.TryParse(userIdClaim.Value, out var userId))
            return;

        var hasPermission = await _permissionService.HasPermission(
            userId, requirement.Permission, CancellationToken.None);

        if (hasPermission)
            context.Succeed(requirement);
    }
}
