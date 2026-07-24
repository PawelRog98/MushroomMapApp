using System.Security.Claims;
using MediatR;
using MushroomMapApp.Features.Users.GetPermissions;
using MushroomMapApp.Features.Users.Login;
using MushroomMapApp.Features.Users.Logout;
using MushroomMapApp.Features.Users.Refresh;
using MushroomMapApp.Features.Users.Register;
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
    }
}
