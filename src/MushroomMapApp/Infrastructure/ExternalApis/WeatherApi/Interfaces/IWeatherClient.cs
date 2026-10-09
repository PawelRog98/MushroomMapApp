using MushroomMapApp.Features.Weather.GetWeather;
using MushroomMapApp.Infrastructure.ExternalApis.WeatherApi.Models;

namespace MushroomMapApp.Infrastructure.ExternalApis.WeatherApi.Interfaces;

public interface IWeatherClient
{
    Task<WeatherDataDto> GetWeather(double latitude, double longitude, CancellationToken cancellationToken = default);
}
