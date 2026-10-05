using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MushroomMap.IntegrationTests.Fixtures;
using MushroomMap.IntegrationTests.Helpers;
using MushroomMapApp.Features.Users.Login;
using Xunit;

namespace MushroomMap.IntegrationTests.Features.Users;

public class LogoutTests : IntegrationTestBase
{
    public LogoutTests(IntegrationTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Logout_WithValidToken_RevokesRefreshToken()
    {
        var user = await CreateUserAsync();

        var loginResponse = await user.Client.PostAsJsonAsync("/api/users/login",
            new { email = user.Email, password = TestData.Password });
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var login = await loginResponse.ReadApiAsync<AuthTokenDto>();
        var refreshToken = login.Data!.RefreshToken;

        var logoutResponse = await user.Client.PostAsync("/api/users/logout", null);

        logoutResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var logoutContent = await logoutResponse.ReadApiAsync<string>();
        logoutContent.Success.Should().BeTrue();
        logoutContent.Data.Should().Be("Logged out successfully.");

        var refreshResponse = await user.Client.PostAsJsonAsync("/api/users/refresh", new { refreshToken });

        refreshResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var refreshContent = await refreshResponse.ReadApiAsync<object>();
        refreshContent.Message.Should().Be("Invalid refresh token.");
    }

    [Fact]
    public async Task Logout_WithoutToken_ReturnsUnauthorized()
    {
        var client = CreateClient();

        var response = await client.PostAsync("/api/users/logout", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
