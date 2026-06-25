using System.Security.Claims;
using MediatR;
using MushroomMapApp.Features.Reactions.AddUserReaction;
using MushroomMapApp.Features.Reactions.GetReactionsForLocation;
using MushroomMapApp.Shared.Response;

namespace MushroomMapApp.Features.Reactions;

public static class Endpoints
{
    public static void ReactionsEndpoints(this IEndpointRouteBuilder app)
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
    }
}
