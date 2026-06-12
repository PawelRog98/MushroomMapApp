using MediatR;
using MushroomMapApp.Domain.Models;
using MushroomMapApp.Shared.Response;

namespace MushroomMapApp.Features.Users.Login;

public static class Endpoints
{
    public static void MapLoginEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/users/login", async (LoginRequest request, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new Command(request), cancellationToken);
            return ApiResponse.Ok(result);  
        })
        .WithTags("Users")
        .Produces<Response<AuthTokenDto>>(StatusCodes.Status200OK)
        .Produces<ErrorResponse>(StatusCodes.Status400BadRequest);
    }
}
