using FluentAssertions;
using FluentValidation.TestHelper;
using MushroomMapApp.Features.Users.CreateUserLocation;
using Xunit;

namespace MushroomMap.UnitTests.Features.Users;

public class CreateUserLocationCommandValidatorTests
{
    private readonly CommandValidator _validator = new(new Validator());

    private static CreateUserLocationCommand ValidCommand() => new(
        new CreateUserLocationRequest(52.2297, 21.0122, null),
        UserId: 1);

    [Fact]
    public void Validate_ValidCommand_HasNoErrors()
    {
        var result = _validator.TestValidate(ValidCommand());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_NullRequest_HasError()
    {
        var result = _validator.TestValidate(new CreateUserLocationCommand(null!, 1));

        result.ShouldHaveValidationErrorFor(x => x.Request);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_UserIdNotPositive_HasError(long userId)
    {
        var result = _validator.TestValidate(ValidCommand() with { UserId = userId });

        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void Validate_InvalidCoordinatesInRequest_HasError()
    {
        var command = new CreateUserLocationCommand(
            new CreateUserLocationRequest(999, 999, null),
            UserId: 1);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Request.Lat);
        result.ShouldHaveValidationErrorFor(x => x.Request.Lng);
    }
}
