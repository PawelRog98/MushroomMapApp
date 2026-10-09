using FluentAssertions;
using FluentValidation.TestHelper;
using MushroomMapApp.Features.Locations.CreateLocation;
using Xunit;

namespace MushroomMap.UnitTests.Features.Locations;

public class CreateLocationValidatorTests
{
    private readonly Validator _validator = new();

    private static CreateLocationRequest ValidRequest() => new()
    {
        Name = "Test Location",
        Text = "Test description",
        Lat = 50.0,
        Lng = 19.0
    };

    [Fact]
    public void Validate_ValidRequest_HasNoErrors()
    {
        var result = _validator.TestValidate(ValidRequest());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validate_EmptyName_HasError(string? name)
    {
        var result = _validator.TestValidate(new CreateLocationRequest { Name = name!, Text = "Test", Lat = 50.0, Lng = 19.0 });

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_NameTooLong_HasError()
    {
        var result = _validator.TestValidate(new CreateLocationRequest { Name = new string('a', 513), Text = "Test", Lat = 50.0, Lng = 19.0 });

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validate_EmptyText_HasError(string? text)
    {
        var result = _validator.TestValidate(new CreateLocationRequest { Name = "Test", Text = text!, Lat = 50.0, Lng = 19.0 });

        result.ShouldHaveValidationErrorFor(x => x.Text);
    }

    [Fact]
    public void Validate_TextTooLong_HasError()
    {
        var result = _validator.TestValidate(new CreateLocationRequest { Name = "Test", Text = new string('a', 4097), Lat = 50.0, Lng = 19.0 });

        result.ShouldHaveValidationErrorFor(x => x.Text);
    }

    [Fact]
    public void Validate_ZeroLat_Passes()
    {
        var result = _validator.TestValidate(new CreateLocationRequest { Name = "Test", Text = "Test", Lat = 0, Lng = 19.0 });

        result.ShouldNotHaveValidationErrorFor(x => x.Lat);
    }

    [Fact]
    public void Validate_ZeroLng_Passes()
    {
        var result = _validator.TestValidate(new CreateLocationRequest { Name = "Test", Text = "Test", Lat = 50.0, Lng = 0 });

        result.ShouldNotHaveValidationErrorFor(x => x.Lng);
    }
}
