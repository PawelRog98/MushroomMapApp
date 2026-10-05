using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MushroomMap.IntegrationTests.Fixtures;
using MushroomMap.IntegrationTests.Helpers;
using MushroomMapApp.Features.Users.Login;
using Xunit;

namespace MushroomMap.IntegrationTests.Features.Users;

public class RefreshTests : IntegrationTestBase
{
    public RefreshTests(IntegrationTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Refresh_WithValidToken_ReturnsNewTokens()
    {
        var client = CreateClient();
        var login = await LoginAsync(client, TestData.AdminEmail);

        var response = await client.PostAsJsonAsync("/api/users/refresh",
            new { refreshToken = login.RefreshToken });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.ReadApiAsync<AuthTokenDto>();
        content.Success.Should().BeTrue();
        content.Data.Should().NotBeNull();
        content.Data!.AccessToken.Should().NotBeNullOrWhiteSpace();
        content.Data.RefreshToken.Should().NotBeNullOrWhiteSpace();
        content.Data.RefreshToken.Should().NotBe(login.RefreshToken);
    }

    [Fact]
    public async Task Refresh_WithIssuedToken_ReturnsUsableAccessToken()
    {
        var client = CreateClient();
        var login = await LoginAsync(client, TestData.AdminEmail);

        var refreshResponse = await client.PostAsJsonAsync("/api/users/refresh",
            new { refreshToken = login.RefreshToken });
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var refreshed = await refreshResponse.ReadApiAsync<AuthTokenDto>();

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/users/get-permissions");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Bearer", refreshed.Data!.AccessToken);

        var permissionsResponse = await client.SendAsync(request);

        permissionsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Refresh_WithAlreadyUsedToken_ReturnsBadRequest()
    {
        var client = CreateClient();
        var login = await LoginAsync(client, TestData.AdminEmail);

        var firstRefresh = await client.PostAsJsonAsync("/api/users/refresh",
            new { refreshToken = login.RefreshToken });
        firstRefresh.StatusCode.Should().Be(HttpStatusCode.OK);

        var secondRefresh = await client.PostAsJsonAsync("/api/users/refresh",
            new { refreshToken = login.RefreshToken });

        secondRefresh.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await secondRefresh.ReadApiAsync<object>();
        content.Success.Should().BeFalse();
        content.Message.Should().Be("Invalid refresh token.");
    }

    [Fact]
    public async Task Refresh_WithUnknownToken_ReturnsBadRequest()
    {
        var client = CreateClient();

        var response = await client.PostAsJsonAsync("/api/users/refresh",
            new { refreshToken = Guid.NewGuid().ToString() });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.ReadApiAsync<object>();
        content.Message.Should().Be("Invalid refresh token.");
    }
}
