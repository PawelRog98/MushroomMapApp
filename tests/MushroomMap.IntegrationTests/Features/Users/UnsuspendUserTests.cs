using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MushroomMap.IntegrationTests.Fixtures;
using MushroomMap.IntegrationTests.Helpers;
using MushroomMapApp.Features.Users.GetAllUsers;
using Xunit;

namespace MushroomMap.IntegrationTests.Features.Users;

public class UnsuspendUserTests : IntegrationTestBase
{
    public UnsuspendUserTests(IntegrationTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task UnsuspendUser_ForSuspendedUser_LiftsSuspension()
    {
        var admin = await CreateAdminClientAsync();
        var target = await CreateUserAsync();

        var suspendResponse = await admin.PostAsJsonAsync("/api/users/suspend",
            new { userPublicId = target.PublicId, days = 5, reason = "Temporary" });
        suspendResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var suspended = (await GetUsersAsync(admin)).Single(u => u.PublicId == target.PublicId);
        suspended.IsActiveSuspension.Should().BeTrue();

        var response = await admin.PostAsJsonAsync("/api/users/unsuspend",
            new { userPublicId = target.PublicId });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.ReadApiAsync<string>();
        content.Success.Should().BeTrue();
        content.Data.Should().Be("User unsuspended successfully.");

        var unsuspended = (await GetUsersAsync(admin)).Single(u => u.PublicId == target.PublicId);
        unsuspended.IsActiveSuspension.Should().BeFalse();
        unsuspended.SuspensionEndDate.Should().BeNull();
    }

    [Fact]
    public async Task UnsuspendUser_ForUnknownUser_ReturnsBadRequest()
    {
        var admin = await CreateAdminClientAsync();

        var response = await admin.PostAsJsonAsync("/api/users/unsuspend",
            new { userPublicId = Guid.NewGuid() });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.ReadApiAsync<object>();
        content.Message.Should().Be("Invalid user data.");
    }

    [Fact]
    public async Task UnsuspendUser_AsRegularUser_ReturnsForbidden()
    {
        var user = await CreateUserAsync();

        var response = await user.Client.PostAsJsonAsync("/api/users/unsuspend",
            new { userPublicId = Guid.NewGuid() });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private async Task<List<UserListItemDto>> GetUsersAsync(HttpClient client)
    {
        var response = await client.GetAsync("/api/users/get-all?page=1&pageSize=100");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.ReadApiPayloadAsync();
        return payload.GetProperty("data").ToObject<List<UserListItemDto>>()!;
    }
}
