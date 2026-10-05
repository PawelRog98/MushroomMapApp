using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MushroomMap.IntegrationTests.Fixtures;
using MushroomMap.IntegrationTests.Helpers;
using MushroomMapApp.Features.Users.GetUserData;
using Xunit;

namespace MushroomMap.IntegrationTests.Features.Users;

public class GetUserDataTests : IntegrationTestBase
{
    public GetUserDataTests(IntegrationTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task GetUserData_WithExistingUser_ReturnsUserData()
    {
        var user = await CreateUserAsync();

        var response = await user.Client.GetAsync($"/api/users/get-user-data?userPublicId={user.PublicId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.ReadApiAsync<UserDataDto>();
        content.Success.Should().BeTrue();
        content.Data.Should().NotBeNull();
        content.Data!.UserId.Should().Be(user.PublicId);
        content.Data.Email.Should().Be(user.Email);
        content.Data.PublicNick.Should().Be(user.Nick);
        content.Data.FirstName.Should().Be("Test");
        content.Data.LastName.Should().Be("User");
        content.Data.RoleName.Should().Be("User");
        content.Data.IsEmailConfirmed.Should().BeTrue();
    }

    [Fact]
    public async Task GetUserData_WithUnknownUser_ReturnsNotFound()
    {
        var client = await CreateAdminClientAsync();

        var response = await client.GetAsync($"/api/users/get-user-data?userPublicId={Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var content = await response.ReadApiAsync<object>();
        content.Success.Should().BeFalse();
        content.Message.Should().Be("User not found.");
    }

    [Fact]
    public async Task GetUserData_WithoutToken_ReturnsUnauthorized()
    {
        var client = CreateClient();

        var response = await client.GetAsync($"/api/users/get-user-data?userPublicId={Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
