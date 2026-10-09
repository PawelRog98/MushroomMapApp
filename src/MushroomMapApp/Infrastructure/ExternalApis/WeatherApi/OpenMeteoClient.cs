using System.Globalization;
using Microsoft.AspNetCore.WebUtilities;
using MushroomMapApp.Features.Weather.GetWeather;
using MushroomMapApp.Infrastructure.ExternalApis.WeatherApi.Interfaces;
using MushroomMapApp.Infrastructure.ExternalApis.WeatherApi.Models;

namespace MushroomMapApp.Infrastructure.ExternalApis.WeatherApi;

public class OpenMeteoClient : IWeatherClient
{
    private readonly HttpClient _httpClient;
    public OpenMeteoClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<WeatherDataDto> GetWeather(double latitude, double longitude, CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, string?>
        {
            ["latitude"] = latitude.ToString(CultureInfo.InvariantCulture),
            ["longitude"] = longitude.ToString(CultureInfo.InvariantCulture),
            ["hourly"] = "temperature_2m,precipitation,wind_speed_10m,weather_code",
            ["forecast_days"] = "1"
        };

        var url = QueryHelpers.AddQueryString("v1/forecast", query);

        var response = await _httpClient.GetAsync(url, cancellationToken);

        response.EnsureSuccessStatusCode();
        var data =  await response.Content.ReadFromJsonAsync<OpenMeteoResponse>(cancellationToken);

        return MapResult(data);
    }

    private static WeatherDataDto MapResult(OpenMeteoResponse weather)
        => new WeatherDataDto
        {
            Time = weather.OpenMeteoHourlyData.Time[0],
            Temperature = weather.OpenMeteoHourlyData.Temperature[0],
            Precipitation =  weather.OpenMeteoHourlyData.Precipitation[0],
            WeatherCode = weather.OpenMeteoHourlyData.WeatherCode[0],
            WindSpeed = weather.OpenMeteoHourlyData.WindSpeed[0]
        };
}
