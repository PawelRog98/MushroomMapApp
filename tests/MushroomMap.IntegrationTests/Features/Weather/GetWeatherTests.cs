using System.Net;
using FluentAssertions;
using MushroomMap.IntegrationTests.Fixtures;
using MushroomMap.IntegrationTests.Helpers;
using MushroomMapApp.Features.Weather.GetWeather;
using Xunit;

namespace MushroomMap.IntegrationTests.Features.Weather;

public class GetWeatherTests : IntegrationTestBase
{
    public GetWeatherTests(IntegrationTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task GetWeather_WithoutToken_ReturnsUnauthorized()
    {
        var client = CreateClient();

        var response = await client.GetAsync("/api/weather/get-weather?Latitude=50&Longitude=19");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetWeather_WithCoordinates_BindsQueryParametersAndReturnsWeatherData()
    {
        var client = await CreateAdminClientAsync();
        var latitude = Random.Shared.Next(-89, 90);
        var longitude = Random.Shared.Next(-179, 180);

        var response = await client.GetAsync(
            $"/api/weather/get-weather?Latitude={latitude}&Longitude={longitude}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.ReadApiAsync<WeatherDataDto>();
        content.Success.Should().BeTrue();
        content.Data.Should().NotBeNull();
        content.Data!.Temperature.Should().Be(21.5);

        Fixture.Factory.WeatherClient.LastLatitude.Should().Be(latitude);
        Fixture.Factory.WeatherClient.LastLongitude.Should().Be(longitude);
    }

    [Fact]
    public async Task GetWeather_WithInvalidLatitude_ReturnsBadRequest()
    {
        var client = await CreateAdminClientAsync();

        var response = await client.GetAsync("/api/weather/get-weather?Latitude=not-a-number&Longitude=19");

        (await response.ReadApiStatusCodeAsync()).Should().Be(400);
    }
}
