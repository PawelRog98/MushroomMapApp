using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MushroomMap.IntegrationTests.Fixtures;
using MushroomMap.IntegrationTests.Helpers;
using MushroomMapApp.Domain.Enums;
using Xunit;

namespace MushroomMap.IntegrationTests.Features.Users;

public class CreateVerificationTokenTests : IntegrationTestBase
{
    public CreateVerificationTokenTests(IntegrationTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task CreateVerificationToken_ForExistingUser_SendsVerificationEmail()
    {
        var user = await CreateUserAsync();

        var response = await user.Client.PostAsJsonAsync("/api/users/create-verification-token",
            new { email = user.Email });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.ReadApiAsync<string>();
        content.Success.Should().BeTrue();
        content.Data.Should().Be("Verification token created successfully.");

        var expectedCode = await Fixture.QueryDbAsync(db => db.Tokens
            .Where(t => t.TokenData != string.Empty
                        && t.TokenTypeValue == TokenType.ActivationToken.ToString()
                        && t.User.Email == user.Email)
            .OrderByDescending(t => t.ExpireDateTime)
            .Select(t => t.TokenData)
            .FirstAsync());

        var deadline = DateTime.UtcNow.AddSeconds(30);
        while (DateTime.UtcNow < deadline
               && !Fixture.Factory.EmailService.Sent.Any(m => m.To == user.Email && m.HtmlBody.Contains(expectedCode)))
        {
            await Task.Delay(250);
        }

        var message = Fixture.Factory.EmailService.Sent
            .FirstOrDefault(m => m.To == user.Email && m.HtmlBody.Contains(expectedCode));

        message.Should().NotBeNull("the verification job must deliver the new code by email");
        message!.Subject.Should().Be("Verification Code");
        message.HtmlBody.Should().Contain(expectedCode);
    }

    [Fact]
    public async Task CreateVerificationToken_ForUnknownUser_ReturnsNotFound()
    {
        var client = CreateClient();

        var response = await client.PostAsJsonAsync("/api/users/create-verification-token",
            new { email = TestData.UniqueEmail("unknown") });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var content = await response.ReadApiAsync<object>();
        content.Success.Should().BeFalse();
        content.Message.Should().Be("User not found.");
    }
}
