using FluentAssertions;
using FluentValidation.TestHelper;
using MushroomMapApp.Features.Users.Register;
using Xunit;

namespace MushroomMap.UnitTests.Features.Users;

public class RegisterValidatorTests
{
    private readonly Validator _validator = new();

    private static RegisterRequest ValidRequest() => new(
        "test@test.com",
        "nickname",
        "John",
        "Doe",
        "Password123!",
        "Password123!",
        new DateTime(1990, 1, 1));

    [Fact]
    public void Validate_ValidRequest_HasNoErrors()
    {
        var result = _validator.TestValidate(ValidRequest());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validate_EmptyEmail_HasError(string? email)
    {
        var request = ValidRequest();
        var result = _validator.TestValidate(request with { Email = email! });

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("test@")]
    public void Validate_InvalidEmail_HasError(string email)
    {
        var request = ValidRequest();
        var result = _validator.TestValidate(request with { Email = email });

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validate_EmptyPublicNick_HasError(string? nick)
    {
        var request = ValidRequest();
        var result = _validator.TestValidate(request with { PublicNick = nick! });

        result.ShouldHaveValidationErrorFor(x => x.PublicNick);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validate_EmptyFirstName_HasError(string? firstName)
    {
        var request = ValidRequest();
        var result = _validator.TestValidate(request with { FirstName = firstName! });

        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validate_EmptyLastName_HasError(string? lastName)
    {
        var request = ValidRequest();
        var result = _validator.TestValidate(request with { LastName = lastName! });

        result.ShouldHaveValidationErrorFor(x => x.LastName);
    }

    [Theory]
    [InlineData("short")]
    [InlineData("1234567")]
    public void Validate_ShortPassword_HasError(string password)
    {
        var request = ValidRequest();
        var result = _validator.TestValidate(request with { Password = password, ConfirmPassword = password });

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validate_PasswordMismatch_HasError()
    {
        var request = ValidRequest();
        var result = _validator.TestValidate(request with { ConfirmPassword = "DifferentPassword123!" });

        result.ShouldHaveValidationErrorFor(x => x.ConfirmPassword);
    }

    [Fact]
    public void Validate_NullDateOfBirth_HasError()
    {
        var request = ValidRequest();
        var result = _validator.TestValidate(request with { DateOfBirth = null! });

        result.ShouldHaveValidationErrorFor(x => x.DateOfBirth);
    }
}
