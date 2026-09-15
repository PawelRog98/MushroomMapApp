using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using MushroomMapApp.Domain.Entities;
using MushroomMapApp.Domain.Permissions;
using MushroomMapApp.Features.Users.AttachPermissions;
using MushroomMapApp.Features.Users.GetPermissions;
using MushroomMapApp.Features.Users.Login;
using MushroomMapApp.Features.Users.Logout;
using MushroomMapApp.Features.Users.Refresh;
using MushroomMapApp.Features.Users.Register;
using MushroomMapApp.Features.Users.GetAllUsers;
using MushroomMapApp.Features.Users.GetUserData;
using MushroomMapApp.Features.Users.Suspend;
using MushroomMapApp.Features.Users.Unsuspend;
using MushroomMapApp.Features.Users.UpdateAvatar;
using MushroomMapApp.Features.Users.UpdateUserData;
using MushroomMapApp.Shared.Response;

namespace MushroomMapApp.Features.Users;

public static class Endpoints
{
    public static void MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/users").WithTags("Users");

        group.MapPost("login", async (LoginRequest request, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new MushroomMapApp.Features.Users.Login.Command(request), cancellationToken);
            return ApiResponse.Ok(result);
        })
        .Produces<Response<AuthTokenDto>>(StatusCodes.Status200OK)
        .Produces<ErrorResponse>(StatusCodes.Status400BadRequest);

        group.MapPost("register", async (RegisterRequest request, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new MushroomMapApp.Features.Users.Register.Command(request), cancellationToken);
            return ApiResponse.Ok(result);
        })
        .Produces<Response<string>>(StatusCodes.Status200OK)
        .Produces<ErrorResponse>(StatusCodes.Status400BadRequest);

        group.MapPost("refresh", async (RefreshRequest request, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new Refresh.Command(request), cancellationToken);
            return ApiResponse.Ok<AuthTokenDto>(result);
        })
        .Produces<Response<AuthTokenDto>>(StatusCodes.Status200OK)
        .Produces<ErrorResponse>(StatusCodes.Status400BadRequest);

        group.MapPost("logout", async (ClaimsPrincipal user, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim is null || !long.TryParse(userIdClaim, out var userId))
                return ApiResponse.BadRequest("User not found.");

            await mediator.Send(new MushroomMapApp.Features.Users.Logout.Command(userId), cancellationToken);
            return ApiResponse.Ok<string>("Logged out successfully.");
        })
        .RequireAuthorization()
        .Produces<Response<string>>(StatusCodes.Status200OK);

        group.MapGet("get-permissions", async (ClaimsPrincipal user, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim is null || !long.TryParse(userIdClaim, out var userId))
                return ApiResponse.BadRequest("User not found.");

            var result = await mediator.Send(new GetPermissionsQuery(userId), cancellationToken);
            return ApiResponse.Ok(result);
        })
        .RequireAuthorization()
        .Produces<Response<UserPermissionsDto>>(StatusCodes.Status200OK)
        .Produces<ErrorResponse>(StatusCodes.Status400BadRequest);

        group.MapGet("get-all",
            async (IMediator mediator, CancellationToken cancellationToken) =>
            {
                var result = await mediator.Send(new GetAllUsersQuery(), cancellationToken);
                return ApiResponse.Ok(result);
            })
            .RequireAuthorization(Permissions.AdministratorDashboard.UserManagment.Code)
            .Produces<Response<List<UserListItemDto>>>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest);

        group.MapPost("suspend",
            async (SuspendUserRequest request, ClaimsPrincipal user, IMediator mediator, CancellationToken cancellationToken) =>
            {
                var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userIdClaim is null || !long.TryParse(userIdClaim, out var userId))
                    return ApiResponse.BadRequest("User not found.");

                var result = await mediator.Send(new SuspendUserCommand(userId, request), cancellationToken);
                return ApiResponse.Ok(result);
            })
            .RequireAuthorization(Permissions.AdministratorDashboard.UserManagment.Code)
            .Produces<Response<object>>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest);

        group.MapPost("unsuspend",
            async (UnsuspendUserRequest request, IMediator mediator, CancellationToken cancellationToken) =>
            {
                await mediator.Send(new UnsuspendUserCommand(request), cancellationToken);
                return ApiResponse.Ok<object>("User unsuspended successfully.");
            })
            .RequireAuthorization(Permissions.AdministratorDashboard.UserManagment.Code)
            .Produces<Response<object>>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest);

        group.MapPost("set-permission",
                async (AttachPermissionsRequest request, ClaimsPrincipal user, IMediator Mediator,
                    CancellationToken cancellationToken) =>
                {
                    var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (userIdClaim is null || !long.TryParse(userIdClaim, out var userId))
                        return ApiResponse.BadRequest("User not found.");

                    var result = await Mediator.Send(new AttachPermissionsCommand(userId, request), cancellationToken);
                    return ApiResponse.Ok(result);
                })
            .RequireAuthorization(Permissions.AdministratorDashboard.PermissionsEdit.Code)
            .Produces<Response<object>>( StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest);

        group.MapGet("get-user-data",
            async (Guid userPublicId, IMediator Mediator, CancellationToken cancellationToken) =>
            {
                var result = await Mediator.Send(new GetUserDataQuery(new GetUserDataRequest(userPublicId)), cancellationToken);
                return ApiResponse.Ok(result);
            })
            .RequireAuthorization()
            .Produces<Response<UserDataDto>>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest);

        group.MapPut("update-user-data",
                async (UpdateUserDataRequest request, ClaimsPrincipal user, IMediator Mediator,
                    CancellationToken cancellationToken) =>
                {
                    var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (userIdClaim is null || !long.TryParse(userIdClaim, out var userId))
                        return ApiResponse.BadRequest("User not found.");

                    var result = await Mediator.Send(new UpdateUserDataUpdateCommand(request, userId),
                        cancellationToken);
                    return ApiResponse.Ok(result);
                })
            .RequireAuthorization()
            .Produces<Response<object>>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest);

        group.MapPost("update-avatar",
            async ([FromForm] UpdateAvatarRequest request, ClaimsPrincipal user, IMediator Mediator,
                CancellationToken cancellationToken) =>
            {
                var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userIdClaim is null || !long.TryParse(userIdClaim, out var userId))
                    return ApiResponse.BadRequest("User not found.");

                if (request.Image == null)
                    return ApiResponse.Ok(apiMessage: "No image found.");

                var result = await Mediator.Send(new UpdateAvatarCommand(request, userId), cancellationToken);
                return ApiResponse.Ok(result);
            })
            .RequireAuthorization()
            .DisableAntiforgery()
            .Produces<Response<object>>(StatusCodes.Status200OK)
            .Produces<Response<ErrorResponse>>(StatusCodes.Status400BadRequest);
    }
}
