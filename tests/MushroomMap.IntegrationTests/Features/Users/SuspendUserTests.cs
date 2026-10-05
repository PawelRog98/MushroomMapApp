using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MushroomMap.IntegrationTests.Fixtures;
using MushroomMap.IntegrationTests.Helpers;
using MushroomMapApp.Features.Users.GetAllUsers;
using Xunit;

namespace MushroomMap.IntegrationTests.Features.Users;

public class SuspendUserTests : IntegrationTestBase
{
    public SuspendUserTests(IntegrationTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task SuspendUser_AsAdministrator_SuspendsUser()
    {
        var admin = await CreateAdminClientAsync();
        var target = await CreateUserAsync();

        var response = await admin.PostAsJsonAsync("/api/users/suspend",
            new { userPublicId = target.PublicId, days = 5, reason = "Integration test suspension" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.ReadApiAsync<object>();
        content.Success.Should().BeTrue();

        var entry = (await GetUsersAsync(admin)).Single(u => u.PublicId == target.PublicId);
        entry.IsActiveSuspension.Should().BeTrue();
        entry.SuspensionEndDate.Should().NotBeNull();
        entry.SuspensionEndDate!.Value.Date.Should().Be(DateTime.UtcNow.AddDays(5).Date);
    }

    [Fact]
    public async Task SuspendUser_ForUnknownUser_ReturnsBadRequest()
    {
        var admin = await CreateAdminClientAsync();

        var response = await admin.PostAsJsonAsync("/api/users/suspend",
            new { userPublicId = Guid.NewGuid(), days = 5, reason = "Unknown user" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.ReadApiAsync<object>();
        content.Success.Should().BeFalse();
        content.Message.Should().Be("Invalid user data.");
    }

    [Fact]
    public async Task SuspendUser_AsRegularUser_ReturnsForbidden()
    {
        var user = await CreateUserAsync();

        var response = await user.Client.PostAsJsonAsync("/api/users/suspend",
            new { userPublicId = Guid.NewGuid(), days = 1, reason = "Not allowed" });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task SuspendUser_WithoutToken_ReturnsUnauthorized()
    {
        var client = CreateClient();

        var response = await client.PostAsJsonAsync("/api/users/suspend",
            new { userPublicId = Guid.NewGuid(), days = 1, reason = "No token" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private async Task<List<UserListItemDto>> GetUsersAsync(HttpClient client)
    {
        var response = await client.GetAsync("/api/users/get-all?page=1&pageSize=100");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.ReadApiPayloadAsync();
        return payload.GetProperty("data").ToObject<List<UserListItemDto>>()!;
    }
}
