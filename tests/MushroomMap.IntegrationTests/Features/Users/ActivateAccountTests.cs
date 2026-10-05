using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MushroomMap.IntegrationTests.Fixtures;
using MushroomMap.IntegrationTests.Helpers;
using MushroomMapApp.Domain.Enums;
using MushroomMapApp.Features.Users.Register;
using Xunit;

namespace MushroomMap.IntegrationTests.Features.Users;

public class ActivateAccountTests : IntegrationTestBase
{
    public ActivateAccountTests(IntegrationTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task ActivateAccount_WithValidCode_ActivatesAccount()
    {
        var client = CreateClient();
        var email = TestData.UniqueEmail("activate");

        var registerResponse = await client.PostAsJsonAsync("/api/users/register", new RegisterRequest(
            email, TestData.UniqueNick("act"), "Test", "User",
            TestData.Password, TestData.Password, DateTime.UtcNow.AddYears(-25)));
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var activationResponse = await client.PostAsJsonAsync("/api/users/activate-account",
            new { code = await ReadActivationCodeAsync(email), email });

        activationResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await activationResponse.ReadApiAsync<object>();
        content.Success.Should().BeTrue();

        var isConfirmed = await Fixture.QueryDbAsync(db => db.Users
            .Where(u => u.Email == email)
            .Select(u => u.IsEmailConfirmed)
            .FirstAsync());
        isConfirmed.Should().BeTrue();

        var loginResponse = await client.PostAsJsonAsync("/api/users/login",
            new { email, password = TestData.Password });
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ActivateAccount_WithInvalidCode_ReturnsUnauthorized()
    {
        var client = CreateClient();

        var response = await client.PostAsJsonAsync("/api/users/activate-account",
            new { code = "DOES-NOT-EXIST", email = TestData.UniqueEmail("missing") });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var content = await response.ReadApiAsync<object>();
        content.Success.Should().BeFalse();
        content.Message.Should().Be("Token not found");
    }

    [Fact]
    public async Task ActivateAccount_WithCodeOfAnotherUser_ReturnsUnauthorized()
    {
        var client = CreateClient();
        var ownerEmail = TestData.UniqueEmail("owner");
        var otherEmail = TestData.UniqueEmail("other");

        var registerResponse = await client.PostAsJsonAsync("/api/users/register", new RegisterRequest(
            ownerEmail, TestData.UniqueNick("own"), "Test", "User",
            TestData.Password, TestData.Password, DateTime.UtcNow.AddYears(-25)));
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var response = await client.PostAsJsonAsync("/api/users/activate-account",
            new { code = await ReadActivationCodeAsync(ownerEmail), email = otherEmail });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var content = await response.ReadApiAsync<object>();
        content.Message.Should().Be("Token not found");
    }

    private Task<string> ReadActivationCodeAsync(string email)
        => Fixture.QueryDbAsync(db => db.Tokens
            .Where(t => t.TokenData != string.Empty
                        && t.TokenTypeValue == TokenType.ActivationToken.ToString()
                        && t.User.Email == email
                        && t.ExpireDateTime > DateTime.UtcNow)
            .OrderByDescending(t => t.ExpireDateTime)
            .Select(t => t.TokenData)
            .FirstAsync());
}
