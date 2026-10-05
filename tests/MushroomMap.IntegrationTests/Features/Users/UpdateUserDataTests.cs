using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MushroomMap.IntegrationTests.Fixtures;
using MushroomMap.IntegrationTests.Helpers;
using MushroomMapApp.Features.Users.GetUserData;
using Xunit;

namespace MushroomMap.IntegrationTests.Features.Users;

public class UpdateUserDataTests : IntegrationTestBase
{
    public UpdateUserDataTests(IntegrationTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task UpdateUserData_WithValidData_UpdatesProfile()
    {
        var user = await CreateUserAsync();
        var dateOfBirth = new DateTime(1995, 4, 12, 0, 0, 0, DateTimeKind.Utc);

        var response = await user.Client.PutAsJsonAsync("/api/users/update-user-data", new
        {
            publicNick = "UpdatedNick",
            firstName = "Updated",
            lastName = "Profile",
            dateOfBirth,
            accountInfo = "Updated account info"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.ReadApiAsync<object>();
        content.Success.Should().BeTrue();

        var profile = await user.Client.GetAsync($"/api/users/get-user-data?userPublicId={user.PublicId}");
        profile.StatusCode.Should().Be(HttpStatusCode.OK);

        var data = (await profile.ReadApiAsync<UserDataDto>()).Data!;
        data.PublicNick.Should().Be("UpdatedNick");
        data.FirstName.Should().Be("Updated");
        data.LastName.Should().Be("Profile");
        data.AccountInfo.Should().Be("Updated account info");
        data.DateOfBirth.Should().Be(dateOfBirth);
    }

    [Fact]
    public async Task UpdateUserData_WithFutureDateOfBirth_ReturnsValidationErrors()
    {
        var user = await CreateUserAsync();

        var response = await user.Client.PutAsJsonAsync("/api/users/update-user-data", new
        {
            publicNick = user.Nick,
            firstName = "Test",
            lastName = "User",
            dateOfBirth = DateTime.UtcNow.AddYears(1),
            accountInfo = string.Empty
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.ReadApiAsync<object>();
        content.Message.Should().Be("Validation failed");
        content.Errors.Should().ContainSingle().Which.Should().Contain("Date Of Birth");
    }

    [Fact]
    public async Task UpdateUserData_WithEmptyNick_ReturnsValidationErrors()
    {
        var user = await CreateUserAsync();

        var response = await user.Client.PutAsJsonAsync("/api/users/update-user-data", new
        {
            publicNick = string.Empty,
            firstName = "Test",
            lastName = "User",
            dateOfBirth = new DateTime(1995, 4, 12, 0, 0, 0, DateTimeKind.Utc),
            accountInfo = string.Empty
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.ReadApiAsync<object>();
        content.Message.Should().Be("Validation failed");
        content.Errors.Should().Contain("'Public Nick' must not be empty.");
    }

    [Fact]
    public async Task UpdateUserData_WithoutToken_ReturnsUnauthorized()
    {
        var client = CreateClient();

        var response = await client.PutAsJsonAsync("/api/users/update-user-data", new
        {
            publicNick = "Nick",
            firstName = "First",
            lastName = "Last",
            dateOfBirth = new DateTime(1995, 4, 12, 0, 0, 0, DateTimeKind.Utc),
            accountInfo = string.Empty
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
