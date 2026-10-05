using System.Net;
using FluentAssertions;
using MushroomMap.IntegrationTests.Fixtures;
using MushroomMap.IntegrationTests.Helpers;
using MushroomMapApp.Features.Files.DownloadImage;
using Xunit;

namespace MushroomMap.IntegrationTests.Features.Files;

public class GetImageTests : IntegrationTestBase
{
    public GetImageTests(IntegrationTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task GetImage_WithExistingImage_ReturnsImageBytes()
    {
        var user = await CreateUserAsync();
        var location = await user.Client.CreateLocationAsync();
        var imageId = (await user.Client.GetLocationsAsync(location.Name))
            .Single()
            .Data.Images
            .Single()
            .PublicId;

        var response = await user.Client.GetAsync($"/api/files/get-image/{imageId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/json");

        var content = await response.ReadApiAsync<ImageResultDto>();
        content.Success.Should().BeTrue();
        content.Data.Should().NotBeNull();
        content.Data!.FileBytes.Should().NotBeEmpty();
        content.Data.ContentType.Should().Be(TestImages.PngContentType);
        content.Data.FileName.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task GetImage_WithUnknownImage_ReturnsNotFound()
    {
        var client = await CreateAdminClientAsync();

        var response = await client.GetAsync($"/api/files/get-image/{Guid.NewGuid()}");

        (await response.ReadApiStatusCodeAsync()).Should().Be(404);
        var content = await response.ReadApiAsync<ImageResultDto>();
        content.Success.Should().BeFalse();
        content.Message.Should().Be("Resource not found");
    }

    [Fact]
    public async Task GetImage_WithoutToken_ReturnsUnauthorized()
    {
        var client = CreateClient();

        var response = await client.GetAsync($"/api/files/get-image/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
