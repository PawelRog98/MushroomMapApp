using System.Security.Claims;
using MediatR;
using MushroomMapApp.Features.Reactions.AddUserReaction;
using MushroomMapApp.Features.Reactions.GetReactionsForLocation;
using MushroomMapApp.Features.Reactions.GetReactionTypes;
using MushroomMapApp.Shared.Response;

namespace MushroomMapApp.Features.Reactions;

public static class Endpoints
{
    public static void MapReactionsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/reactions").WithTags("Reactions");

        group.MapPost("add-reaction",
            async (AddReactionRequest request, ClaimsPrincipal user, IMediator mediator,
                CancellationToken cancellationToken) =>
            {
                var userIdStr = user.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrWhiteSpace(userIdStr) || !long.TryParse(userIdStr, out var userId))
                    return ApiResponse.Forbidden("User not found");

                var response = await mediator.Send(new AddLocationReactionCommand(request, userId), cancellationToken);
                return ApiResponse.Ok(response);

            })
            .RequireAuthorization()
            .Produces<Response<object>>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden);

        group.MapGet("get-reactions/{locationPublicId:guid}",
            async (Guid locationPublicId, IMediator mediator, CancellationToken cancellationToken) =>
            {
                var response = await mediator.Send(
                    new GetReactionsForLocationCommand(new GetReactionsForLocationRequest(locationPublicId)),
                    cancellationToken);
                return ApiResponse.Ok(response);
            })
            .RequireAuthorization()
            .Produces<Response<IEnumerable<ReactionDto>>>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest);

        group.MapGet("types",
            async (IMediator mediator, CancellationToken cancellationToken) =>
            {
                var response = await mediator.Send(new GetReactionTypesQuery(), cancellationToken);
                return ApiResponse.Ok(response);
            })
            .Produces<Response<IEnumerable<ReactionTypeDto>>>(StatusCodes.Status200OK);
    }
}
