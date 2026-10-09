using MushroomMapApp.Features.Weather.GetWeather;
using MushroomMapApp.Infrastructure.ExternalApis.WeatherApi.Interfaces;

namespace MushroomMap.IntegrationTests.Fixtures;

public class FakeWeatherClient : IWeatherClient
{
    public double? LastLatitude { get; private set; }
    public double? LastLongitude { get; private set; }

    public Task<WeatherDataDto> GetWeather(double latitude, double longitude, CancellationToken cancellationToken = default)
    {
        LastLatitude = latitude;
        LastLongitude = longitude;

        return Task.FromResult(new WeatherDataDto
        {
            Time = "2026-10-07T12:00:00",
            Temperature = 21.5,
            Precipitation = 0,
            WindSpeed = 12.3,
            WeatherCode = 1
        });
    }
}
