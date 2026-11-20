using System.Net;
using FluentAssertions;
using MushroomMap.IntegrationTests.Fixtures;
using MushroomMap.IntegrationTests.Helpers;
using MushroomMapApp.Features.Locations.GetLocations;
using Xunit;

namespace MushroomMap.IntegrationTests.Features.Locations;

public class GetLocationsTests : IntegrationTestBase
{
    public GetLocationsTests(IntegrationTestFixture fixture) : base(fixture)
    {
    }

    private static long CellIndex(double coordinate, int zoom)
        => (long)Math.Floor(coordinate / MapGrid.CellSizeDegrees(zoom));

    [Fact]
    public async Task GetLocations_WithLocationInBoundingBox_ReturnsMarkerWithPermissions()
    {
        var admin = await CreateAdminClientAsync();
        var created = await admin.CreateLocationAsync(TestData.UniqueLocationName("Visible"));

        var result = await admin.GetMapLocationsAsync();

        result.Mode.Should().Be("markers");
        result.TotalCount.Should().BeGreaterThanOrEqualTo(1);
        result.Truncated.Should().BeFalse();

        var location = result.Markers.Single(l => l.Data.PublicId == created.PublicId);
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
    public async Task GetLocations_WithBoundingBoxOutsideLocation_ReturnsEmptyMarkers()
    {
        var admin = await CreateAdminClientAsync();
        await admin.CreateLocationAsync(TestData.UniqueLocationName("Poland"), lat: 50.0, lng: 19.0);

        var response = await admin.GetAsync(
            "/api/locations/get-locations?west=100&east=110&south=10&north=20&zoom=16");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.ReadApiPayloadAsync();
        var result = payload.GetProperty("data").ToObject<MapLocationsPayload>();

        result.Should().NotBeNull();
        result!.Mode.Should().Be("markers");
        result.TotalCount.Should().Be(0);
        result.Markers.Should().BeEmpty();
    }

    [Fact]
    public async Task GetLocations_AtLowZoom_ReturnsClusters()
    {
        var admin = await CreateAdminClientAsync();
        await admin.CreateLocationAsync(TestData.UniqueLocationName("Clustered"));

        var result = await admin.GetMapLocationsAsync(zoom: MapGrid.DefaultZoom);

        result.Mode.Should().Be("clusters");
        result.TotalCount.Should().BeGreaterThanOrEqualTo(1);
        result.Markers.Should().BeEmpty();
        result.Clusters.Should().NotBeEmpty();
        result.Clusters.Should().OnlyContain(c => c.Count > 0);
        result.Clusters.Should().OnlyContain(c => c.Zoom == MapGrid.DefaultZoom);
    }

    [Fact]
    public async Task GetClusterLocations_WithLocationInCell_ReturnsLocationWithPermissions()
    {
        var admin = await CreateAdminClientAsync();
        var created = await admin.CreateLocationAsync(TestData.UniqueLocationName("In cell"));

        var result = await admin.GetClusterLocationsAsync(
            CellIndex(created.Lng, MapGrid.MaxZoom),
            CellIndex(created.Lat, MapGrid.MaxZoom));

        result.TotalCount.Should().BeGreaterThanOrEqualTo(1);
        result.CurrentPage.Should().Be(1);

        var location = result.Items.Single(l => l.Data.PublicId == created.PublicId);
        location.Data.Name.Should().Be(created.Name);
        location.Data.Lat.Should().Be(created.Lat);
        location.Data.Lng.Should().Be(created.Lng);
        location.Meta.CanView.Should().BeTrue();
        location.Meta.CanEdit.Should().BeTrue();
        location.Meta.CanDelete.Should().BeTrue();
    }

    [Fact]
    public async Task GetClusterLocations_WithSearch_ReturnsOnlyMatchingLocations()
    {
        var admin = await CreateAdminClientAsync();
        var prefix = TestData.UniqueLocationName("ClusterSearch");
        var matching = await admin.CreateLocationAsync(prefix);
        await admin.CreateLocationAsync(TestData.UniqueLocationName("Different"), lat: matching.Lat + 0.0001, lng: matching.Lng + 0.0001, includeImage: false);

        var result = await admin.GetClusterLocationsAsync(
            CellIndex(matching.Lng, MapGrid.MaxZoom),
            CellIndex(matching.Lat, MapGrid.MaxZoom),
            search: prefix);

        result.TotalCount.Should().Be(1);
        result.Items.Should().ContainSingle()
            .Which.Data.PublicId.Should().Be(matching.PublicId);
    }

    [Fact]
    public async Task GetClusterLocations_WithPaging_ReturnsPagingMetadata()
    {
        var admin = await CreateAdminClientAsync();
        var prefix = TestData.UniqueLocationName("ClusterPage");
        for (var i = 0; i < 3; i++)
        {
            await admin.CreateLocationAsync($"{prefix} {i}", lat: 50.0 + i * 0.002, lng: 19.0 + i * 0.002, includeImage: false);
        }

        var result = await admin.GetClusterLocationsAsync(
            CellIndex(19.0, MapGrid.DefaultZoom),
            CellIndex(50.0, MapGrid.DefaultZoom),
            zoom: MapGrid.DefaultZoom,
            search: prefix,
            page: 2,
            pageSize: 2);

        result.TotalCount.Should().Be(3);
        result.CurrentPage.Should().Be(2);
        result.PageSize.Should().Be(2);
        result.TotalPages.Should().Be(2);
        result.HasPrevious.Should().BeTrue();
        result.HasNext.Should().BeFalse();
        result.Items.Should().ContainSingle();
    }

    [Fact]
    public async Task GetClusterLocations_WithoutToken_ReturnsUnauthorized()
    {
        var client = CreateClient();

        var response = await client.GetAsync("/api/locations/get-cluster-locations?cellX=0&cellY=0&zoom=10");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetLocations_WithoutToken_ReturnsUnauthorized()
    {
        var client = CreateClient();

        var response = await client.GetAsync(ApiEndpointsExtensions.WorldBoundingBox);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
