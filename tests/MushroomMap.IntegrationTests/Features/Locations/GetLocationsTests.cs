using System.Net;
using FluentAssertions;
using MushroomMap.IntegrationTests.Fixtures;
using MushroomMap.IntegrationTests.Helpers;
using Xunit;

namespace MushroomMap.IntegrationTests.Features.Locations;

public class GetLocationsTests : IntegrationTestBase
{
    public GetLocationsTests(IntegrationTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task GetLocations_WithLocationInBoundingBox_ReturnsLocationWithPermissions()
    {
        var admin = await CreateAdminClientAsync();
        var created = await admin.CreateLocationAsync(TestData.UniqueLocationName("Visible"));

        var locations = await admin.GetLocationsAsync();

        var location = locations.Single(l => l.Data.PublicId == created.PublicId);
        location.Data.Name.Should().Be(created.Name);
        location.Data.Lat.Should().Be(created.Lat);
        location.Data.Lng.Should().Be(created.Lng);
        location.Meta.CanView.Should().BeTrue();
        location.Meta.CanEdit.Should().BeTrue();
        location.Meta.CanDelete.Should().BeTrue();
    }

    [Fact]
    public async Task GetLocations_WithSearch_ReturnsOnlyMatchingLocations()
    {
        var admin = await CreateAdminClientAsync();
        var uniqueName = TestData.UniqueLocationName("Searchable");
        await admin.CreateLocationAsync(uniqueName);
        await admin.CreateLocationAsync(TestData.UniqueLocationName("Other"));

        var locations = await admin.GetLocationsAsync(uniqueName);

        locations.Should().ContainSingle()
            .Which.Data.Name.Should().Be(uniqueName);
    }

    [Fact]
    public async Task GetLocations_WithBoundingBoxOutsideLocation_ReturnsEmptyList()
    {
        var admin = await CreateAdminClientAsync();
        await admin.CreateLocationAsync(TestData.UniqueLocationName("Poland"), lat: 50.0, lng: 19.0);

        var response = await admin.GetAsync(
            "/api/locations/get-locations?west=100&east=110&south=10&north=20");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.ReadApiPayloadAsync();
        payload.GetProperty("data").EnumerateArray().Should().BeEmpty();
    }

    [Fact]
    public async Task GetLocations_WithoutToken_ReturnsUnauthorized()
    {
        var client = CreateClient();

        var response = await client.GetAsync(ApiEndpointsExtensions.WorldBoundingBox);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
