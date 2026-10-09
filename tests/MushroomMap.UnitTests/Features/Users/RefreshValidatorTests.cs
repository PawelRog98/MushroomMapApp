using FluentAssertions;
using FluentValidation.TestHelper;
using MushroomMapApp.Features.Users.Refresh;
using Xunit;

namespace MushroomMap.UnitTests.Features.Users;

public class RefreshValidatorTests
{
    private readonly RefreshRequestValidator _validator = new();

    [Fact]
    public void Validate_ValidRequest_HasNoErrors()
    {
        var result = _validator.TestValidate(new RefreshRequest("valid-token"));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validate_EmptyRefreshToken_HasError(string? token)
    {
        var result = _validator.TestValidate(new RefreshRequest(token!));

        result.ShouldHaveValidationErrorFor(x => x.RefreshToken);
    }
}
