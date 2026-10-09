using MediatR;
using MushroomMapApp.Domain.Data;
using MushroomMapApp.Domain.Interfaces;
using MushroomMapApp.Infrastructure.ExternalApis.WeatherApi.Interfaces;

namespace MushroomMapApp.Features.Weather.GetWeather;

public record GetWeatherRequest(double Latitude, double Longitude);
public record GetWeatherQuery(GetWeatherRequest Request) : IRequest<WeatherDataDto>;
public class GetWeatherQueryHandler : IRequestHandler<GetWeatherQuery, WeatherDataDto>
{
    private readonly IWeatherClient _weatherClient;
    private readonly IRedisCache _redisCache;

    public GetWeatherQueryHandler(IWeatherClient weatherClient, IRedisCache redisCache)
    {
        _weatherClient = weatherClient;
        _redisCache = redisCache;
    }

    public async Task<WeatherDataDto> Handle(GetWeatherQuery request, CancellationToken cancellationToken)
    {
        var cahceKey = $"weather:{request.Request.Latitude}:{request.Request.Longitude}";

        var cached = await _redisCache.GetAsync<WeatherDataDto>(cahceKey, cancellationToken);

        if(cached != null)
            return  cached;

        var result = await _weatherClient.GetWeather(request.Request.Latitude, request.Request.Longitude, cancellationToken);

        await _redisCache.SetAsync(cahceKey, result, TimeSpan.FromMinutes(30), cancellationToken);

        return result;

    }
}
