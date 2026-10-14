using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MushroomMap.IntegrationTests.Fixtures;
using MushroomMap.IntegrationTests.Helpers;
using MushroomMapApp.Features.Users.CreateUserLocation;
using Xunit;

namespace MushroomMap.IntegrationTests.Features.Users;

public class GetMyLocationTests : IntegrationTestBase
{
    public GetMyLocationTests(IntegrationTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task GetMyLocation_WithoutSavedLocation_ReturnsNullData()
    {
        var user = await CreateUserAsync();

        var response = await user.Client.GetAsync("/api/users/get-my-location");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.ReadApiAsync<UserLocationDto?>();
        content.Success.Should().BeTrue();
        content.Data.Should().BeNull();
    }

    [Fact]
    public async Task GetMyLocation_AfterCreate_ReturnsSavedLocation()
    {
        var user = await CreateUserAsync();

        var createResponse = await user.Client.PostAsJsonAsync(
            "/api/users/create-user-location",
            new { lat = 52.2297, lng = 21.0122, publicId = (Guid?)null });
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var response = await user.Client.GetAsync("/api/users/get-my-location");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.ReadApiAsync<UserLocationDto>();
        content.Success.Should().BeTrue();
        content.Data.Should().NotBeNull();
        content.Data!.LocationName.Should().Be("My location");
        content.Data.Lat.Should().Be(52.2297);
        content.Data.Lng.Should().Be(21.0122);
        content.Data.PublicId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task GetMyLocation_ReturnsOwnLocationOnly()
    {
        var userWithLocation = await CreateUserAsync();
        var otherUser = await CreateUserAsync();

        var createResponse = await userWithLocation.Client.PostAsJsonAsync(
            "/api/users/create-user-location",
            new { lat = 52.2297, lng = 21.0122, publicId = (Guid?)null });
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var response = await otherUser.Client.GetAsync("/api/users/get-my-location");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.ReadApiAsync<UserLocationDto?>();
        content.Success.Should().BeTrue();
        content.Data.Should().BeNull();
    }

    [Fact]
    public async Task GetMyLocation_WithoutToken_ReturnsUnauthorized()
    {
        var client = CreateClient();

        var response = await client.GetAsync("/api/users/get-my-location");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
