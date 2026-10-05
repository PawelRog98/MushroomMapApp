using System.Net;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MushroomMap.IntegrationTests.Fixtures;
using MushroomMap.IntegrationTests.Helpers;
using MushroomMapApp.Features.Users.GetUserData;
using Xunit;

namespace MushroomMap.IntegrationTests.Features.Users;

public class UpdateAvatarTests : IntegrationTestBase
{
    public UpdateAvatarTests(IntegrationTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task UpdateAvatar_WithPngImage_SetsAvatar()
    {
        var user = await CreateUserAsync();

        var response = await user.Client.PostAsync("/api/users/update-avatar", TestImages.CreateAvatarForm());

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.ReadApiAsync<object>();
        content.Success.Should().BeTrue();

        var profile = await ReadProfileAsync(user);
        profile.AvatarUrl.Should().NotBeNullOrWhiteSpace();
        profile.AvatarThumbnailUrl.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task UpdateAvatar_WithUnsupportedContentType_ReturnsValidationErrors()
    {
        var user = await CreateUserAsync();

        var response = await user.Client.PostAsync(
            "/api/users/update-avatar",
            TestImages.CreateAvatarForm("application/pdf"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.ReadApiAsync<object>();
        content.Message.Should().Be("Validation failed");
        content.Errors.Should().Contain("Only JPEG, PNG, and WebP images are allowed.");
    }

    [Fact]
    public async Task UpdateAvatar_WhenReplaced_KeepsOnlyLatestAvatarResources()
    {
        var user = await CreateUserAsync();

        var firstResponse = await user.Client.PostAsync("/api/users/update-avatar", TestImages.CreateAvatarForm());
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var firstAvatarUrl = (await ReadProfileAsync(user)).AvatarUrl;

        var secondResponse = await user.Client.PostAsync("/api/users/update-avatar", TestImages.CreateAvatarForm());
        secondResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var profile = await ReadProfileAsync(user);
        profile.AvatarUrl.Should().NotBe(firstAvatarUrl);

        var userId = await Fixture.QueryDbAsync(db => db.Users
            .Where(u => u.Email == user.Email)
            .Select(u => u.Id)
            .FirstAsync());

        var resourceCount = await Fixture.QueryDbAsync(db => db.FileResources
            .CountAsync(f => f.UserId == userId));

        resourceCount.Should().Be(2);
    }

    [Fact]
    public async Task UpdateAvatar_WithoutToken_ReturnsUnauthorized()
    {
        var client = CreateClient();

        var response = await client.PostAsync("/api/users/update-avatar", TestImages.CreateAvatarForm());

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private async Task<UserDataDto> ReadProfileAsync(TestUser user)
    {
        var response = await user.Client.GetAsync($"/api/users/get-user-data?userPublicId={user.PublicId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.ReadApiAsync<UserDataDto>();
        content.Success.Should().BeTrue();
        return content.Data!;
    }
}
