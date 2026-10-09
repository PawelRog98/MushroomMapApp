using System.Text.Json.Serialization;

namespace MushroomMapApp.Infrastructure.ExternalApis.WeatherApi.Models;

public class OpenMeteoResponse
{
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    [JsonPropertyName("timezone")]
    public string Timezone { get; init; }

    [JsonPropertyName("hourly")]
    public OpenMeteoHourlyData OpenMeteoHourlyData { get; init; }
}

public class OpenMeteoHourlyData
{
    [JsonPropertyName("time")]
    public List<string> Time { get; init; } = [];

    [JsonPropertyName("temperature_2m")]
    public List<double> Temperature { get; init; } = [];

    [JsonPropertyName("precipitation")]
    public List<double> Precipitation { get; init; } = [];

    [JsonPropertyName("wind_speed_10m")]
    public List<double> WindSpeed { get; init; } = [];

    [JsonPropertyName("weather_code")]
    public List<int> WeatherCode { get; init; } = [];
}
