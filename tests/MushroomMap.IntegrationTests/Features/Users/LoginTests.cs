using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MushroomMap.IntegrationTests.Fixtures;
using MushroomMap.IntegrationTests.Helpers;
using MushroomMapApp.Features.Users.Login;
using MushroomMapApp.Features.Users.Register;
using Xunit;

namespace MushroomMap.IntegrationTests.Features.Users;

public class LoginTests : IntegrationTestBase
{
    public LoginTests(IntegrationTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsTokens()
    {
        var client = CreateClient();

        var response = await client.PostAsJsonAsync("/api/users/login",
            new { email = TestData.AdminEmail, password = TestData.Password });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.ReadApiAsync<AuthTokenDto>();
        content.Success.Should().BeTrue();
        content.Message.Should().Be("Request succeeded");
        content.Data.Should().NotBeNull();
        content.Data!.AccessToken.Should().NotBeNullOrWhiteSpace();
        content.Data.RefreshToken.Should().NotBeNullOrWhiteSpace();
        content.Data.UserNick.Should().Be("Admin");
        content.Data.UserId.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_WithUnknownEmail_ReturnsBadRequest()
    {
        var client = CreateClient();

        var response = await client.PostAsJsonAsync("/api/users/login",
            new { email = TestData.UniqueEmail("ghost"), password = TestData.Password });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.ReadApiAsync<object>();
        content.Success.Should().BeFalse();
        content.Message.Should().Be("Invalid user data.");
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsBadRequest()
    {
        var client = CreateClient();

        var response = await client.PostAsJsonAsync("/api/users/login",
            new { email = TestData.AdminEmail, password = "Wrong-Password-1" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.ReadApiAsync<object>();
        content.Message.Should().Be("Invalid user data.");
    }

    [Fact]
    public async Task Login_WithUnconfirmedAccount_ReturnsUnauthorized()
    {
        var client = CreateClient();
        var email = TestData.UniqueEmail("unconfirmed");

        var registerResponse = await client.PostAsJsonAsync("/api/users/register", new RegisterRequest(
            email, TestData.UniqueNick("unc"), "Test", "User",
            TestData.Password, TestData.Password, DateTime.UtcNow.AddYears(-25)));
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var response = await client.PostAsJsonAsync("/api/users/login",
            new { email, password = TestData.Password });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var content = await response.ReadApiAsync<object>();
        content.Success.Should().BeFalse();
        content.Message.Should().Be("User is not active");
    }

    [Fact]
    public async Task Login_WithInvalidPayload_ReturnsValidationErrors()
    {
        var client = CreateClient();

        var response = await client.PostAsJsonAsync("/api/users/login",
            new { email = "not-an-email", password = string.Empty });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.ReadApiAsync<object>();
        content.Message.Should().Be("Validation failed");
        content.Errors.Should().Contain("'Email' is not a valid email address.");
        content.Errors.Should().Contain("'Password' must not be empty.");
    }
}
