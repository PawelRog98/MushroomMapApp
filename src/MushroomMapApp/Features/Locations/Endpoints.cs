using System.Security.Claims;
using MediatR;
using MushroomMapApp.Features.Locations.CreateLocation;
using MushroomMapApp.Features.Locations.GetLocations;
using MushroomMapApp.Features.Locations.UpdateLocation;
using MushroomMapApp.Features.Locations.DeleteLocation;
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
                    return ApiResponse.Forbidden("User not found");

                var response = await mediator.Send(new CreateLocationCommand(request, userId), cancellationToken);
                return ApiResponse.Ok(response);
            })
            .RequireAuthorization()
            .Produces<Response<LocationDto>>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden);

        group.MapGet("get-locations",
            async ([AsParameters] GetLocationRequest request, IMediator mediator, CancellationToken cancellationToken) =>
            {
                var response = await mediator.Send(new GetLocationQuery(request), cancellationToken);
                return ApiResponse.Ok(response);
            })
            .RequireAuthorization()
            .Produces<Response<LocationDto>>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest);

        group.MapPut("update-location/{id:guid}",
            async (Guid id, UpdateLocationRequest request, ClaimsPrincipal user, IMediator mediator, CancellationToken cancellationToken) =>
            {
                var userIdStr = user.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrWhiteSpace(userIdStr) || !long.TryParse(userIdStr, out var userId))
                    return ApiResponse.Forbidden("User not found");

                var response = await mediator.Send(new UpdateLocationCommand(id, request, userId), cancellationToken);
                return ApiResponse.Ok(response);
            })
            .RequireAuthorization()
            .Produces<Response<LocationDto>>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        group.MapDelete("delete-location/{id:guid}",
            async (Guid id, ClaimsPrincipal user, IMediator mediator, CancellationToken cancellationToken) =>
            {
                var userIdStr = user.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrWhiteSpace(userIdStr) || !long.TryParse(userIdStr, out var userId))
                    return ApiResponse.Forbidden("User not found");

                await mediator.Send(new DeleteLocationCommand(id, userId), cancellationToken);
                return ApiResponse.Ok(apiMessage: "Location deleted successfully");
            })
            .RequireAuthorization()
            .Produces<Response<object>>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);
    }
}

