using System.Net;
using FluentAssertions;
using MushroomMap.IntegrationTests.Fixtures;
using MushroomMap.IntegrationTests.Helpers;
using MushroomMapApp.Domain.Permissions;
using MushroomMapApp.Features.Users.GetPermissions;
using Xunit;

namespace MushroomMap.IntegrationTests.Features.Users;

public class GetPermissionsTests : IntegrationTestBase
{
    public GetPermissionsTests(IntegrationTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task GetPermissions_AsAdministrator_ReturnsAllPermissions()
    {
        var client = await CreateAdminClientAsync();

        var response = await client.GetAsync("/api/users/get-permissions");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.ReadApiAsync<UserPermissionsDto>();
        content.Success.Should().BeTrue();
        content.Data!.Permissions.Should().BeEquivalentTo(Permissions.All);
    }

    [Fact]
    public async Task GetPermissions_AsRegularUser_ReturnsLocationsView()
    {
        var user = await CreateUserAsync();

        var response = await user.Client.GetAsync("/api/users/get-permissions");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.ReadApiAsync<UserPermissionsDto>();
        content.Success.Should().BeTrue();
        content.Data!.Permissions.Should().BeEquivalentTo(new[] { Permissions.Locations.View.Code });
    }

    [Fact]
    public async Task GetPermissions_WithoutToken_ReturnsUnauthorized()
    {
        var client = CreateClient();

        var response = await client.GetAsync("/api/users/get-permissions");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
