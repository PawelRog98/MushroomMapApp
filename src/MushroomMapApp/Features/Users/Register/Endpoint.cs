using MediatR;
using MushroomMapApp.Shared.Response;

namespace MushroomMapApp.Features.Users.Register;

public static class Endpoints
{
    public static void MapRegisterEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/users/register", async (RegisterRequest request, IMediator mediator,  CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new Command(request), cancellationToken);
            return ApiResponse.Ok(result);
        })
        .WithTags("Users")
        .Produces<Response<string>>(StatusCodes.Status200OK)
        .Produces<ErrorResponse>(StatusCodes.Status400BadRequest);
    }
}
