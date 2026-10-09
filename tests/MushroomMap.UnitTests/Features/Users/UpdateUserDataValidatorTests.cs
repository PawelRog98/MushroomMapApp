using FluentAssertions;
using FluentValidation.TestHelper;
using MushroomMapApp.Features.Users.UpdateUserData;
using Xunit;

namespace MushroomMap.UnitTests.Features.Users;

public class UpdateUserDataValidatorTests
{
    private readonly Validator _validator = new();

    private static UpdateUserDataRequest ValidRequest() => new(
        "nickname",
        "John",
        "Doe",
        new DateTime(1990, 1, 1),
        "Account info");

    [Fact]
    public void Validate_ValidRequest_HasNoErrors()
    {
        var result = _validator.TestValidate(ValidRequest());

        result.ShouldNotHaveAnyValidationErrors();
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

    [Fact]
    public void Validate_PublicNickTooLong_HasError()
    {
        var request = ValidRequest();
        var result = _validator.TestValidate(request with { PublicNick = new string('a', 129) });

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

    [Fact]
    public void Validate_FirstNameTooLong_HasError()
    {
        var request = ValidRequest();
        var result = _validator.TestValidate(request with { FirstName = new string('a', 129) });

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

    [Fact]
    public void Validate_LastNameTooLong_HasError()
    {
        var request = ValidRequest();
        var result = _validator.TestValidate(request with { LastName = new string('a', 129) });

        result.ShouldHaveValidationErrorFor(x => x.LastName);
    }

    [Fact]
    public void Validate_FutureDateOfBirth_HasError()
    {
        var request = ValidRequest();
        var result = _validator.TestValidate(request with { DateOfBirth = DateTime.UtcNow.AddDays(1) });

        result.ShouldHaveValidationErrorFor(x => x.DateOfBirth);
    }

    [Fact]
    public void Validate_PastDateOfBirth_Passes()
    {
        var request = ValidRequest();
        var result = _validator.TestValidate(request with { DateOfBirth = DateTime.UtcNow.AddDays(-1) });

        result.ShouldNotHaveValidationErrorFor(x => x.DateOfBirth);
    }

    [Fact]
    public void Validate_AccountInfoTooLong_HasError()
    {
        var request = ValidRequest();
        var result = _validator.TestValidate(request with { AccountInfo = new string('a', 2049) });

        result.ShouldHaveValidationErrorFor(x => x.AccountInfo);
    }

    [Fact]
    public void Validate_NullAccountInfo_Passes()
    {
        var request = ValidRequest();
        var result = _validator.TestValidate(request with { AccountInfo = null! });

        result.ShouldNotHaveValidationErrorFor(x => x.AccountInfo);
    }
}
