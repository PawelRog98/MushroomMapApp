using System.Net;
using FluentAssertions;
using MushroomMapApp.Features.Locations.CreateLocation;
using MushroomMapApp.Features.Locations.GetLocations;
using MushroomMapApp.Features.Reactions.GetReactionTypes;

namespace MushroomMap.IntegrationTests.Helpers;

public static class ApiEndpointsExtensions
{
    public const string WorldBoundingBox =
        "/api/locations/get-locations?west=-180&east=180&south=-90&north=90";

    public static async Task<LocationDto> CreateLocationAsync(
        this HttpClient client,
        string? name = null,
        string text = "Created by an integration test",
        double lat = 50.0614,
        double lng = 19.9366,
        bool includeImage = true)
    {
        using var form = TestImages.CreateLocationForm(name ?? TestData.UniqueLocationName(), text, lat, lng, includeImage);
        var response = await client.PostAsync("/api/locations/create-location", form);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.ReadApiAsync<LocationDto>();
        content.Success.Should().BeTrue();
        content.Data.Should().NotBeNull();

        return content.Data!;
    }

    public static async Task<List<LocationItem>> GetLocationsAsync(this HttpClient client, string? search = null)
    {
        var url = WorldBoundingBox;
        if (!string.IsNullOrEmpty(search))
            url += $"&search={Uri.EscapeDataString(search)}";

        var response = await client.GetAsync(url);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.ReadApiPayloadAsync();
        payload.GetProperty("success").GetBoolean().Should().BeTrue();

        return payload.GetProperty("data").ToObject<List<LocationItem>>()!;
    }

    public static async Task<List<ReactionTypeDto>> GetReactionTypesAsync(this HttpClient client)
    {
        var response = await client.GetAsync("/api/reactions/types");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.ReadApiAsync<List<ReactionTypeDto>>();
        content.Success.Should().BeTrue();

        return content.Data!;
    }
}
