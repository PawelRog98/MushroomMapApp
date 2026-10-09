using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Http;
using MushroomMapApp.Features.Locations.UpdateLocation;
using Xunit;

namespace MushroomMap.UnitTests.Features.Locations;

public class UpdateLocationValidatorTests
{
    private readonly Validator _validator = new();

    private static UpdateLocationRequest ValidRequest() => new(
        "Test Location",
        "Test description",
        new FormFileCollection(),
        new List<Guid>());

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
        var request = ValidRequest();
        var result = _validator.TestValidate(request with { Name = name! });

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_NameTooLong_HasError()
    {
        var request = ValidRequest();
        var result = _validator.TestValidate(request with { Name = new string('a', 513) });

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validate_EmptyText_HasError(string? text)
    {
        var request = ValidRequest();
        var result = _validator.TestValidate(request with { Text = text! });

        result.ShouldHaveValidationErrorFor(x => x.Text);
    }

    [Fact]
    public void Validate_TextTooLong_HasError()
    {
        var request = ValidRequest();
        var result = _validator.TestValidate(request with { Text = new string('a', 4097) });

        result.ShouldHaveValidationErrorFor(x => x.Text);
    }
}
