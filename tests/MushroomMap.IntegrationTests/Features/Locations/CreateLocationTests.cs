using System.Net;
using FluentAssertions;
using MushroomMap.IntegrationTests.Fixtures;
using MushroomMap.IntegrationTests.Helpers;
using MushroomMapApp.Features.Locations.CreateLocation;
using Xunit;

namespace MushroomMap.IntegrationTests.Features.Locations;

public class CreateLocationTests : IntegrationTestBase
{
    public CreateLocationTests(IntegrationTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task CreateLocation_WithImage_CreatesLocationVisibleOnMap()
    {
        var admin = await CreateAdminClientAsync();
        var name = TestData.UniqueLocationName("With image");

        var response = await admin.CreateLocationAsync(name, "A mystical mushroom", lat: 51.1, lng: 17.03);

        response.PublicId.Should().NotBeEmpty();
        response.Name.Should().Be(name);
        response.Text.Should().Be("A mystical mushroom");
        response.Lat.Should().Be(51.1);
        response.Lng.Should().Be(17.03);

        var locations = await admin.GetLocationsAsync(name);
        var location = locations.Single();
        location.Data.PublicId.Should().Be(response.PublicId);
        location.Data.AuthorName.Should().Be("Admin");
        location.Data.Images.Should().HaveCount(1);
        location.Data.Images[0].ThumbnailUrl.Should().NotBeNullOrWhiteSpace();
        location.Meta.CanView.Should().BeTrue();
        location.Meta.CanEdit.Should().BeTrue();
        location.Meta.CanDelete.Should().BeTrue();
    }

    [Fact]
    public async Task CreateLocation_WithoutImage_CreatesLocation()
    {
        var user = await CreateUserAsync();
        var name = TestData.UniqueLocationName("No image");

        var response = await user.Client.CreateLocationAsync(name, includeImage: false);

        response.Name.Should().Be(name);

        var locations = await user.Client.GetLocationsAsync(name);
        locations.Single().Data.Images.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateLocation_AsRegularUser_CreatesLocationOwnedByUser()
    {
        var user = await CreateUserAsync();
        var name = TestData.UniqueLocationName("Owned");

        var response = await user.Client.CreateLocationAsync(name);

        response.Name.Should().Be(name);

        var locations = await user.Client.GetLocationsAsync(name);
        var location = locations.Single();
        location.Data.AuthorName.Should().Be(user.Nick);
        location.Data.AuthorPublicId.Should().Be(user.PublicId);
        location.Meta.CanView.Should().BeTrue();
    }

    [Fact]
    public async Task CreateLocation_WithEmptyName_ReturnsValidationErrors()
    {
        var user = await CreateUserAsync();

        using var form = TestImages.CreateLocationForm(string.Empty, "No name");
        var response = await user.Client.PostAsync("/api/locations/create-location", form);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.ReadApiAsync<object>();
        content.Message.Should().Be("Validation failed");
        content.Errors.Should().Contain("'Name' must not be empty.");
    }

    [Fact]
    public async Task CreateLocation_WithoutToken_ReturnsUnauthorized()
    {
        var client = CreateClient();

        using var form = TestImages.CreateLocationForm(TestData.UniqueLocationName(), "Anonymous");
        var response = await client.PostAsync("/api/locations/create-location", form);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
