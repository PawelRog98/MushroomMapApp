using MediatR;
using MushroomMapApp.Features.Weather.GetWeather;
using MushroomMapApp.Shared.Response;

namespace MushroomMapApp.Features.Weather;

public static class Endpoints
{
    public static void MapWeatherEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/weather").WithTags("Weather");

        group.MapGet("get-weather",
            async ([AsParameters] GetWeatherRequest request, IMediator mediator, CancellationToken cancellationToken) =>
            {
                var result = await mediator.Send(new GetWeatherQuery(request), cancellationToken);

                return ApiResponse.Ok(result);
            })
            .RequireAuthorization()
            .Produces<Response<WeatherDataDto>>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest);
    }
}
