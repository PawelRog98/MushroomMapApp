using FluentAssertions;
using FluentValidation.TestHelper;
using MushroomMapApp.Features.Users.Login;
using Xunit;

namespace MushroomMap.UnitTests.Features.Users;

public class LoginValidatorTests
{
    private readonly Validator _validator = new();

    [Fact]
    public void Validate_ValidRequest_HasNoErrors()
    {
        var result = _validator.TestValidate(new LoginRequest("test@test.com", "password"));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validate_EmptyEmail_HasError(string? email)
    {
        var result = _validator.TestValidate(new LoginRequest(email!, "password"));

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("test@")]
    [InlineData("@test.com")]
    public void Validate_InvalidEmail_HasError(string email)
    {
        var result = _validator.TestValidate(new LoginRequest(email, "password"));

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validate_EmptyPassword_HasError(string? password)
    {
        var result = _validator.TestValidate(new LoginRequest("test@test.com", password!));

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}
