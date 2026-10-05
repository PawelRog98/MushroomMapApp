using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MushroomMap.IntegrationTests.Fixtures;
using MushroomMap.IntegrationTests.Helpers;
using MushroomMapApp.Features.Users.Register;
using Xunit;

namespace MushroomMap.IntegrationTests.Features.Users;

public class RegisterTests : IntegrationTestBase
{
    public RegisterTests(IntegrationTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Register_WithValidUser_RegistersSuccessfully()
    {
        var client = CreateClient();
        var email = TestData.UniqueEmail("register");

        var request = new RegisterRequest(
            email,
            TestData.UniqueNick("reg"),
            "Test",
            "User",
            TestData.Password,
            TestData.Password,
            DateTime.UtcNow.AddYears(-17));

        var response = await client.PostAsJsonAsync("/api/users/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.ReadApiAsync<object>();
        content.Success.Should().BeTrue();
        content.Message.Should().Be("Request succeeded");

        var storedUser = await Fixture.QueryDbAsync(db => db.Users
            .Where(u => u.Email == email)
            .Select(u => new { u.IsEmailConfirmed, u.PublicNick, RoleName = u.Role.Name })
            .FirstOrDefaultAsync());

        storedUser.Should().NotBeNull();
        storedUser!.IsEmailConfirmed.Should().BeFalse();
        storedUser.RoleName.Should().Be("User");
    }

    [Fact]
    public async Task Register_WithExistingEmail_ReturnsBadRequest()
    {
        var client = CreateClient();
        var email = TestData.UniqueEmail("duplicate");

        var request = new RegisterRequest(
            email, TestData.UniqueNick("dup"), "Test", "User",
            TestData.Password, TestData.Password, DateTime.UtcNow.AddYears(-30));

        var firstResponse = await client.PostAsJsonAsync("/api/users/register", request);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var secondResponse = await client.PostAsJsonAsync("/api/users/register", request);

        secondResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await secondResponse.ReadApiAsync<object>();
        content.Success.Should().BeFalse();
        content.Message.Should().Be("A user with such an email already exists.");
    }

    [Fact]
    public async Task Register_WithInvalidPayload_ReturnsValidationErrors()
    {
        var client = CreateClient();

        var request = new RegisterRequest(
            "not-an-email",
            string.Empty,
            string.Empty,
            string.Empty,
            "short",
            "different",
            null);

        var response = await client.PostAsJsonAsync("/api/users/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.ReadApiAsync<object>();
        content.Success.Should().BeFalse();
        content.Message.Should().Be("Validation failed");
        content.Errors.Should().Contain("'Email' is not a valid email address.");
        content.Errors.Should().Contain("The length of 'Password' must be at least 8 characters. You entered 5 characters.");
        content.Errors.Should().Contain("'Confirm Password' must be equal to 'short'.");
        content.Errors.Should().Contain("'Date Of Birth' must not be empty.");
    }

    [Fact]
    public async Task Register_WithValidUser_SendsVerificationEmailAndActivatesAccount()
    {
        var client = CreateClient();
        var email = TestData.UniqueEmail("verify");

        var request = new RegisterRequest(
            email, TestData.UniqueNick("ver"), "Test", "User",
            TestData.Password, TestData.Password, DateTime.UtcNow.AddYears(-25));

        var response = await client.PostAsJsonAsync("/api/users/register", request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var message = await WaitForEmailAsync(email);
        message.Should().NotBeNull();
        message!.Subject.Should().Be("Verification Code");

        var code = ExtractVerificationCode(message.HtmlBody);

        var activationResponse = await client.PostAsJsonAsync("/api/users/activate-account",
            new { code, email });
        activationResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginResponse = await client.PostAsJsonAsync("/api/users/login",
            new { email, password = TestData.Password });
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private static string ExtractVerificationCode(string htmlBody)
    {
        var match = System.Text.RegularExpressions.Regex.Match(htmlBody, "<b>([0-9A-F]+)</b>");
        match.Success.Should().BeTrue("the verification email must contain the activation code");
        return match.Groups[1].Value;
    }
}
