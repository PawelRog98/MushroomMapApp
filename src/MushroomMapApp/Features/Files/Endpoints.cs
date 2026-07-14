using MediatR;
using MushroomMapApp.Features.Files.DownloadImage;
using MushroomMapApp.Shared.Response;

namespace MushroomMapApp.Features.Files;

public static class Endpoints
{
    public static void MapFileEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/files").WithTags("Files");

        group.MapGet("get-image/{publicId:guid}",
            async (Guid publicId, IMediator mediator, CancellationToken cancellationToken) =>
            {
                var result = await mediator.Send(new GetImageQuery(publicId), cancellationToken);
                if (result == null)
                    return ApiResponse.NotFound();

                return ApiResponse.Ok(result);
            })
            .RequireAuthorization()
            .Produces<Response<ImageResultDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }
}
