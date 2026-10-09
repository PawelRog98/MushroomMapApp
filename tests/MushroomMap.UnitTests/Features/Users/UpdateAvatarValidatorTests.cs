using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Http;
using MushroomMapApp.Features.Users.UpdateAvatar;
using Xunit;

namespace MushroomMap.UnitTests.Features.Users;

public class UpdateAvatarValidatorTests
{
    private readonly Validator _validator = new();

    private static IFormFile CreateFormFile(string contentType, long length)
    {
        var stream = new MemoryStream(new byte[length]);
        return new FormFile(stream, 0, length, "Image", "test.jpg")
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }

    [Fact]
    public void Validate_ValidJpeg_Passes()
    {
        var file = CreateFormFile("image/jpeg", 1024);
        var result = _validator.TestValidate(new UpdateAvatarRequest(file));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ValidPng_Passes()
    {
        var file = CreateFormFile("image/png", 1024);
        var result = _validator.TestValidate(new UpdateAvatarRequest(file));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ValidWebp_Passes()
    {
        var file = CreateFormFile("image/webp", 1024);
        var result = _validator.TestValidate(new UpdateAvatarRequest(file));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("image/gif")]
    [InlineData("application/pdf")]
    [InlineData("text/plain")]
    public void Validate_InvalidContentType_HasError(string contentType)
    {
        var file = CreateFormFile(contentType, 1024);
        var result = _validator.TestValidate(new UpdateAvatarRequest(file));

        result.ShouldHaveValidationErrorFor(x => x.Image!.ContentType)
            .WithErrorMessage("Only JPEG, PNG, and WebP images are allowed.");
    }

    [Fact]
    public void Validate_ImageTooLarge_HasError()
    {
        var file = CreateFormFile("image/jpeg", 5 * 1024 * 1024 + 1);
        var result = _validator.TestValidate(new UpdateAvatarRequest(file));

        result.ShouldHaveValidationErrorFor(x => x.Image!.Length)
            .WithErrorMessage("Image must be 5 MB or smaller.");
    }

    [Fact]
    public void Validate_ImageExactlyMaxSize_Passes()
    {
        var file = CreateFormFile("image/jpeg", 5 * 1024 * 1024);
        var result = _validator.TestValidate(new UpdateAvatarRequest(file));

        result.ShouldNotHaveValidationErrorFor(x => x.Image!.Length);
    }
}
