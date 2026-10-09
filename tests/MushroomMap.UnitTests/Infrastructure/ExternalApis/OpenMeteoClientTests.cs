using System.Net;
using System.Text.Json;
using FluentAssertions;
using MushroomMapApp.Features.Weather.GetWeather;
using MushroomMapApp.Infrastructure.ExternalApis.WeatherApi;
using MushroomMapApp.Infrastructure.ExternalApis.WeatherApi.Models;
using Xunit;

namespace MushroomMap.UnitTests.Infrastructure.ExternalApis;

public class OpenMeteoClientTests
{
    private static OpenMeteoResponse CreateResponse() => new()
    {
        Latitude = 50.06,
        Longitude = 19.94,
        Timezone = "Europe/Warsaw",
        OpenMeteoHourlyData = new OpenMeteoHourlyData
        {
            Time = new List<string> { "2026-10-08T12:00" },
            Temperature = new List<double> { 21.5 },
            Precipitation = new List<double> { 0.5 },
            WindSpeed = new List<double> { 12.3 },
            WeatherCode = new List<int> { 1 }
        }
    };

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;

        public StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
        {
            _responder = responder;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(_responder(request));
    }

    [Fact]
    public async Task GetWeather_ReturnsMappedWeatherData()
    {
        var response = CreateResponse();
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(response))
        });

        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.open-meteo.com/") };
        var client = new OpenMeteoClient(httpClient);

        var result = await client.GetWeather(50.06, 19.94);

        result.Should().NotBeNull();
        result.Time.Should().Be("2026-10-08T12:00");
        result.Temperature.Should().Be(21.5);
        result.Precipitation.Should().Be(0.5);
        result.WindSpeed.Should().Be(12.3);
        result.WeatherCode.Should().Be(1);
    }

    [Fact]
    public async Task GetWeather_SendsCorrectQueryParameters()
    {
        var handler = new StubHttpMessageHandler(req =>
        {
            var query = req.RequestUri!.Query;
            query.Should().Contain("latitude=50.06");
            query.Should().Contain("longitude=19.94");
            query.Should().Contain("hourly=temperature_2m,precipitation,wind_speed_10m,weather_code");
            query.Should().Contain("forecast_days=1");

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(CreateResponse()))
            };
        });

        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.open-meteo.com/") };
        var client = new OpenMeteoClient(httpClient);

        await client.GetWeather(50.06, 19.94);
    }

    [Fact]
    public async Task GetWeather_Throws_WhenApiReturnsError()
    {
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("{}")
        });

        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.open-meteo.com/") };
        var client = new OpenMeteoClient(httpClient);

        var act = () => client.GetWeather(50, 19);

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task GetWeather_UsesInvariantCulture_ForCoordinates()
    {
        var handler = new StubHttpMessageHandler(req =>
        {
            var query = req.RequestUri!.Query;
            query.Should().Contain("latitude=-33.8688");
            query.Should().Contain("longitude=151.2093");

            var response = new OpenMeteoResponse
            {
                Latitude = -33.8688,
                Longitude = 151.2093,
                Timezone = "Australia/Sydney",
                OpenMeteoHourlyData = new OpenMeteoHourlyData
                {
                    Time = new List<string> { "2026-10-08T12:00" },
                    Temperature = new List<double> { 25.0 },
                    Precipitation = new List<double> { 0 },
                    WindSpeed = new List<double> { 10 },
                    WeatherCode = new List<int> { 0 }
                }
            };

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(response))
            };
        });

        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.open-meteo.com/") };
        var client = new OpenMeteoClient(httpClient);

        var result = await client.GetWeather(-33.8688, 151.2093);

        result.Temperature.Should().Be(25.0);
    }
}
