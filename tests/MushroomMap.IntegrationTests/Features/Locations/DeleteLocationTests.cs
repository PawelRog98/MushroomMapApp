using System.Net;
using FluentAssertions;
using MushroomMap.IntegrationTests.Fixtures;
using MushroomMap.IntegrationTests.Helpers;
using Xunit;

namespace MushroomMap.IntegrationTests.Features.Locations;

public class DeleteLocationTests : IntegrationTestBase
{
    public DeleteLocationTests(IntegrationTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task DeleteLocation_AsOwner_RemovesLocationFromMap()
    {
        var admin = await CreateAdminClientAsync();
        var created = await admin.CreateLocationAsync(TestData.UniqueLocationName("Removable"));

        var response = await admin.DeleteAsync($"/api/locations/delete-location/{created.PublicId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.ReadApiAsync<object>();
        content.Success.Should().BeTrue();
        content.Message.Should().Be("Location deleted successfully");

        var locations = await admin.GetLocationsAsync(created.Name);
        locations.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteLocation_AsNonOwner_ReturnsForbidden()
    {
        var admin = await CreateAdminClientAsync();
        var intruder = await CreateUserAsync();
        var created = await admin.CreateLocationAsync(TestData.UniqueLocationName("Foreign"));

        var response = await intruder.Client.DeleteAsync($"/api/locations/delete-location/{created.PublicId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        var content = await response.ReadApiAsync<object>();
        content.Success.Should().BeFalse();
        content.Message.Should().Be("You do not have permission to delete this location.");

        var locations = await admin.GetLocationsAsync(created.Name);
        locations.Should().ContainSingle();
    }

    [Fact]
    public async Task DeleteLocation_WithUnknownLocation_ReturnsNotFound()
    {
        var admin = await CreateAdminClientAsync();

        var response = await admin.DeleteAsync($"/api/locations/delete-location/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var content = await response.ReadApiAsync<object>();
        content.Message.Should().Be("Location not found.");
    }

    [Fact]
    public async Task DeleteLocation_WithoutToken_ReturnsUnauthorized()
    {
        var client = CreateClient();

        var response = await client.DeleteAsync($"/api/locations/delete-location/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
