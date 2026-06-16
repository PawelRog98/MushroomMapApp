using MediatR;
using MushroomMapApp.Features.Users.Login;
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
    }
}
