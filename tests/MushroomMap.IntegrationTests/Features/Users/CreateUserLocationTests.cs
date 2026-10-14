using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MushroomMap.IntegrationTests.Fixtures;
using MushroomMap.IntegrationTests.Helpers;
using MushroomMapApp.Features.Users.CreateUserLocation;
using Xunit;

namespace MushroomMap.IntegrationTests.Features.Users;

public class CreateUserLocationTests : IntegrationTestBase
{
    public CreateUserLocationTests(IntegrationTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task CreateUserLocation_WithoutPublicId_CreatesLocation()
    {
        var user = await CreateUserAsync();

        var response = await user.Client.PostAsJsonAsync(
            "/api/users/create-user-location",
            new { lat = 52.2297, lng = 21.0122, publicId = (Guid?)null });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.ReadApiAsync<UserLocationDto>();
        content.Success.Should().BeTrue();
        content.Data.Should().NotBeNull();
        content.Data!.LocationName.Should().Be("My location");
        content.Data.Lat.Should().Be(52.2297);
        content.Data.Lng.Should().Be(21.0122);

        var saved = await Fixture.QueryDbAsync(db => db.UserSavedLocations
            .Where(x => x.User.Email == user.Email)
            .ToListAsync());
        saved.Should().ContainSingle();
        saved[0].LocationName.Should().Be("My location");
        saved[0].Coordinates.Y.Should().Be(52.2297);
        saved[0].Coordinates.X.Should().Be(21.0122);
        saved[0].UpdatedAtUtc.Should().BeNull();
    }

    [Fact]
    public async Task CreateUserLocation_WithExistingPublicId_UpdatesLocation()
    {
        var user = await CreateUserAsync();

        var createResponse = await user.Client.PostAsJsonAsync(
            "/api/users/create-user-location",
            new { lat = 52.2297, lng = 21.0122, publicId = (Guid?)null });
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getResponse = await user.Client.GetAsync("/api/users/get-my-location");
        var existing = await getResponse.ReadApiAsync<UserLocationDto>();
        existing.Data.Should().NotBeNull();

        var updateResponse = await user.Client.PostAsJsonAsync(
            "/api/users/create-user-location",
            new { lat = 50.0614, lng = 19.9366, publicId = existing.Data!.PublicId });

        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updateContent = await updateResponse.ReadApiAsync<UserLocationDto>();
        updateContent.Success.Should().BeTrue();
        updateContent.Data!.PublicId.Should().Be(existing.Data.PublicId);
        updateContent.Data.Lat.Should().Be(50.0614);
        updateContent.Data.Lng.Should().Be(19.9366);

        var saved = await Fixture.QueryDbAsync(db => db.UserSavedLocations
            .Where(x => x.User.Email == user.Email)
            .ToListAsync());
        saved.Should().ContainSingle();
        saved[0].Id.Should().BeGreaterThan(0);
        saved[0].Coordinates.Y.Should().Be(50.0614);
        saved[0].Coordinates.X.Should().Be(19.9366);
        saved[0].UpdatedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateUserLocation_WithUnknownPublicId_ReturnsBadRequest()
    {
        var user = await CreateUserAsync();

        var response = await user.Client.PostAsJsonAsync(
            "/api/users/create-user-location",
            new { lat = 52.2297, lng = 21.0122, publicId = Guid.NewGuid() });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.ReadApiAsync<object>();
        content.Message.Should().Be("Invalid location data.");
    }

    [Fact]
    public async Task CreateUserLocation_WithOutOfRangeCoordinates_ReturnsValidationErrors()
    {
        var user = await CreateUserAsync();

        var response = await user.Client.PostAsJsonAsync(
            "/api/users/create-user-location",
            new { lat = 999.0, lng = 999.0, publicId = (Guid?)null });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.ReadApiAsync<object>();
        content.Message.Should().Be("Validation failed");
        content.Errors.Should().Contain(e => e.Contains("Lat"));
        content.Errors.Should().Contain(e => e.Contains("Lng"));
    }

    [Fact]
    public async Task CreateUserLocation_WithoutToken_ReturnsUnauthorized()
    {
        var client = CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/users/create-user-location",
            new { lat = 52.2297, lng = 21.0122, publicId = (Guid?)null });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
