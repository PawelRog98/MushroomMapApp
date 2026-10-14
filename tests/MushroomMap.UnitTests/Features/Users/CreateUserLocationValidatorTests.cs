using FluentAssertions;
using FluentValidation.TestHelper;
using MushroomMapApp.Features.Users.CreateUserLocation;
using Xunit;

namespace MushroomMap.UnitTests.Features.Users;

public class CreateUserLocationValidatorTests
{
    private readonly Validator _validator = new();

    private static CreateUserLocationRequest ValidRequest() => new(
        Lat: 52.2297,
        Lng: 21.0122,
        PublicId: null);

    [Fact]
    public void Validate_ValidRequest_HasNoErrors()
    {
        var result = _validator.TestValidate(ValidRequest());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ValidRequestWithPublicId_HasNoErrors()
    {
        var result = _validator.TestValidate(ValidRequest() with { PublicId = Guid.NewGuid() });

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(90, 180)]
    [InlineData(-90, -180)]
    [InlineData(52.2297, 21.0122)]
    public void Validate_BoundaryCoordinates_HasNoErrors(double lat, double lng)
    {
        var result = _validator.TestValidate(new CreateUserLocationRequest(lat, lng, null));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(90.0001)]
    [InlineData(-90.0001)]
    [InlineData(120)]
    public void Validate_LatOutOfRange_HasError(double lat)
    {
        var result = _validator.TestValidate(new CreateUserLocationRequest(lat, 21.0, null));

        result.ShouldHaveValidationErrorFor(x => x.Lat);
    }

    [Theory]
    [InlineData(180.0001)]
    [InlineData(-180.0001)]
    [InlineData(200)]
    public void Validate_LngOutOfRange_HasError(double lng)
    {
        var result = _validator.TestValidate(new CreateUserLocationRequest(52.0, lng, null));

        result.ShouldHaveValidationErrorFor(x => x.Lng);
    }
}
