using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MushroomMap.IntegrationTests.Fixtures;
using MushroomMap.IntegrationTests.Helpers;
using MushroomMapApp.Domain.Permissions;
using MushroomMapApp.Features.Users.GetPermissions;
using Xunit;

namespace MushroomMap.IntegrationTests.Features.Users;

public class SetPermissionTests : IntegrationTestBase
{
    public SetPermissionTests(IntegrationTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task SetPermission_AsAdministrator_GrantsPermission()
    {
        var admin = await CreateAdminClientAsync();
        var target = await CreateUserAsync();

        var response = await admin.PostAsJsonAsync("/api/users/set-permission",
            new { userPublicId = target.PublicId, permissions = new[] { Permissions.AdministratorDashboard.View.Code } });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.ReadApiAsync<object>();
        content.Success.Should().BeTrue();

        var permissions = await ReadPermissionsAsync(target.Client);
        permissions.Should().Contain(Permissions.AdministratorDashboard.View.Code);
    }

    [Fact]
    public async Task SetPermission_WhenCalledAgain_ReplacesPreviousPermissions()
    {
        var admin = await CreateAdminClientAsync();
        var target = await CreateUserAsync();

        var grantResponse = await admin.PostAsJsonAsync("/api/users/set-permission",
            new
            {
                userPublicId = target.PublicId,
                permissions = new[]
                {
                    Permissions.AdministratorDashboard.View.Code,
                    Permissions.AdministratorDashboard.UserManagment.Code
                }
            });
        grantResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var replaceResponse = await admin.PostAsJsonAsync("/api/users/set-permission",
            new { userPublicId = target.PublicId, permissions = new[] { Permissions.Locations.View.Code } });
        replaceResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var permissions = await ReadPermissionsAsync(target.Client);
        permissions.Should().BeEquivalentTo(new[] { Permissions.Locations.View.Code });
    }

    [Fact]
    public async Task SetPermission_WithUnknownUser_ReturnsBadRequest()
    {
        var admin = await CreateAdminClientAsync();

        var response = await admin.PostAsJsonAsync("/api/users/set-permission",
            new { userPublicId = Guid.NewGuid(), permissions = new[] { Permissions.Locations.View.Code } });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.ReadApiAsync<object>();
        content.Message.Should().Be("Invalid user data.");
    }

    [Fact]
    public async Task SetPermission_AsRegularUser_ReturnsForbidden()
    {
        var user = await CreateUserAsync();

        var response = await user.Client.PostAsJsonAsync("/api/users/set-permission",
            new { userPublicId = user.PublicId, permissions = new[] { Permissions.Locations.View.Code } });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private async Task<List<string>> ReadPermissionsAsync(HttpClient client)
    {
        var response = await client.GetAsync("/api/users/get-permissions");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.ReadApiAsync<UserPermissionsDto>();
        content.Success.Should().BeTrue();
        return content.Data!.Permissions;
    }
}
