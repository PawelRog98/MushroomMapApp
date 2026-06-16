using System.Security.Claims;
using MediatR;
using MushroomMapApp.Features.Locations.CreateLocation;
using MushroomMapApp.Shared.Response;

namespace MushroomMapApp.Features.Locations;

public static class Endpoints
{
    public static void MapLocationsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/locations").WithTags("Locations");

        group.MapPost("create-location",
            async (CreateLocationRequest request, ClaimsPrincipal user, IMediator mediator, CancellationToken cancellationToken) =>
            {
                var userIdStr = user.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrWhiteSpace(userIdStr) || !long.TryParse(userIdStr, out var userId))
                    return ApiResponse.BadRequest("User not found");

                var response = await mediator.Send(new CreateLocationCommand(request, userId), cancellationToken);
                return ApiResponse.Ok(response);
            })
            .RequireAuthorization()
            .Produces<Response<LocationDto>>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest);
    }
}
