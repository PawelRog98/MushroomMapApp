using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MushroomMap.IntegrationTests.Fixtures;
using MushroomMap.IntegrationTests.Helpers;
using MushroomMapApp.Domain.Enums;
using MushroomMapApp.Domain.Models;
using MushroomMapApp.Features.Users.Login;
using MushroomMapApp.Features.Users.Register;
using Xunit;

namespace MushroomMap.IntegrationTests;

[Collection("IntegrationTests")]
public abstract class IntegrationTestBase
{
    protected IntegrationTestBase(IntegrationTestFixture fixture)
    {
        Fixture = fixture;
    }

    protected IntegrationTestFixture Fixture { get; }

    protected HttpClient CreateClient() => Fixture.CreateClient();

    protected async Task<AuthTokenDto> LoginAsync(HttpClient client, string email, string password = TestData.Password)
    {
        var response = await client.PostAsJsonAsync("/api/users/login", new { email, password });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.ReadApiAsync<AuthTokenDto>();
        payload.Success.Should().BeTrue();
        payload.Data.Should().NotBeNull();

        return payload.Data!;
    }

    protected async Task<HttpClient> CreateAuthorizedClientAsync(string email, string password = TestData.Password)
    {
        var client = CreateClient();
        var token = await LoginAsync(client, email, password);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
        return client;
    }

    protected Task<HttpClient> CreateAdminClientAsync()
        => CreateAuthorizedClientAsync(TestData.AdminEmail);

    protected async Task<TestUser> CreateUserAsync()
    {
        var email = TestData.UniqueEmail();
        var nick = TestData.UniqueNick();
        var client = CreateClient();

        var registerResponse = await client.PostAsJsonAsync("/api/users/register", new RegisterRequest(
            email,
            nick,
            "Test",
            "User",
            TestData.Password,
            TestData.Password,
            DateTime.UtcNow.AddYears(-25)));

        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        await ActivateAccountAsync(client, email);

        var token = await LoginAsync(client, email);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

        var publicId = await Fixture.QueryDbAsync(db => db.Users
            .Where(u => u.Email == email)
            .Select(u => u.PublicId)
            .FirstAsync());

        return new TestUser
        {
            Email = email,
            Nick = nick,
            PublicId = publicId,
            AccessToken = token.AccessToken,
            Client = client
        };
    }

    protected async Task ActivateAccountAsync(HttpClient client, string email)
    {
        var code = await Fixture.QueryDbAsync(db => db.Tokens
            .Where(t => t.TokenData != string.Empty
                        && t.TokenTypeValue == TokenType.ActivationToken.ToString()
                        && t.User.Email == email)
            .OrderByDescending(t => t.ExpireDateTime)
            .Select(t => t.TokenData)
            .FirstAsync());

        var response = await client.PostAsJsonAsync("/api/users/activate-account", new { code, email });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    protected async Task<EmailMessage?> WaitForEmailAsync(string email, TimeSpan? timeout = null)
    {
        var deadline = DateTime.UtcNow + (timeout ?? TimeSpan.FromSeconds(30));

        while (DateTime.UtcNow < deadline)
        {
            var message = Fixture.Factory.EmailService.Sent.FirstOrDefault(m => m.To == email);
            if (message is not null)
                return message;

            await Task.Delay(250);
        }

        return null;
    }
}
