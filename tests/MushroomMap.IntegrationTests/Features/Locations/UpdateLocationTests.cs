using System.Net;
using FluentAssertions;
using MushroomMap.IntegrationTests.Fixtures;
using MushroomMap.IntegrationTests.Helpers;
using MushroomMapApp.Features.Locations.CreateLocation;
using Xunit;

namespace MushroomMap.IntegrationTests.Features.Locations;

public class UpdateLocationTests : IntegrationTestBase
{
    public UpdateLocationTests(IntegrationTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task UpdateLocation_AsOwner_UpdatesLocationAndKeepsImages()
    {
        var admin = await CreateAdminClientAsync();
        var created = await admin.CreateLocationAsync(TestData.UniqueLocationName("Before"));
        var imageIds = await ReadImageIdsAsync(admin, created.Name);
        var updatedName = TestData.UniqueLocationName("After");

        using var form = TestImages.CreateUpdateLocationForm(updatedName, "Updated text", imageIds);
        var response = await admin.PutAsync($"/api/locations/update-location/{created.PublicId}", form);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.ReadApiAsync<LocationDto>();
        content.Success.Should().BeTrue();
        content.Data!.Name.Should().Be(updatedName);
        content.Data.Text.Should().Be("Updated text");
        content.Data.PublicId.Should().Be(created.PublicId);

        var locations = await admin.GetLocationsAsync(updatedName);
        var location = locations.Single();
        location.Data.Text.Should().Be("Updated text");
        location.Data.Images.Should().HaveCount(1);
        location.Data.Images[0].PublicId.Should().Be(imageIds.Single());
    }

    [Fact]
    public async Task UpdateLocation_AsNonOwner_ReturnsForbidden()
    {
        var admin = await CreateAdminClientAsync();
        var intruder = await CreateUserAsync();

        var created = await admin.CreateLocationAsync(TestData.UniqueLocationName("Protected"));
        var imageIds = await ReadImageIdsAsync(admin, created.Name);

        using var form = TestImages.CreateUpdateLocationForm("Hacked", "Hacked", imageIds);
        var response = await intruder.Client.PutAsync($"/api/locations/update-location/{created.PublicId}", form);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        var content = await response.ReadApiAsync<object>();
        content.Success.Should().BeFalse();
        content.Message.Should().Be("You do not have permission to modify this location.");
    }

    [Fact]
    public async Task UpdateLocation_WithUnknownLocation_ReturnsNotFound()
    {
        var admin = await CreateAdminClientAsync();

        using var form = TestImages.CreateUpdateLocationForm("Missing", "Missing", new[] { Guid.NewGuid() });
        var response = await admin.PutAsync($"/api/locations/update-location/{Guid.NewGuid()}", form);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var content = await response.ReadApiAsync<object>();
        content.Message.Should().Be("Location not found.");
    }

    [Fact]
    public async Task UpdateLocation_WithEmptyName_ReturnsValidationErrors()
    {
        var admin = await CreateAdminClientAsync();
        var created = await admin.CreateLocationAsync(TestData.UniqueLocationName("Validated"));

        using var form = TestImages.CreateUpdateLocationForm(string.Empty, "Text", new[] { Guid.NewGuid() });
        var response = await admin.PutAsync($"/api/locations/update-location/{created.PublicId}", form);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.ReadApiAsync<object>();
        content.Message.Should().Be("Validation failed");
        content.Errors.Should().Contain("'Name' must not be empty.");
    }

    [Fact]
    public async Task UpdateLocation_WithoutToken_ReturnsUnauthorized()
    {
        var client = CreateClient();

        using var form = TestImages.CreateUpdateLocationForm("Name", "Text", new[] { Guid.NewGuid() });
        var response = await client.PutAsync($"/api/locations/update-location/{Guid.NewGuid()}", form);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private async Task<List<Guid>> ReadImageIdsAsync(HttpClient client, string locationName)
    {
        var locations = await client.GetLocationsAsync(locationName);
        return locations.Single().Data.Images.Select(i => i.PublicId).ToList();
    }
}
