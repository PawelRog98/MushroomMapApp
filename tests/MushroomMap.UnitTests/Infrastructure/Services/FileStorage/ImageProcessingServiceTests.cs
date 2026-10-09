using FluentAssertions;
using Microsoft.AspNetCore.Http;
using MushroomMapApp.Infrastructure.Services.FileStorage;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

namespace MushroomMap.UnitTests.Infrastructure.Services.FileStorage;

public class ImageProcessingServiceTests
{
    private readonly ImageProcessingService _service = new();

    private static IFormFile CreateFormFile(int width, int height)
    {
        using var image = new Image<Rgba32>(width, height, new Rgba32(30, 120, 60));
        var stream = new MemoryStream();
        image.SaveAsJpeg(stream);
        stream.Position = 0;
        return new FormFile(stream, 0, stream.Length, "Image", "test.jpg")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpeg"
        };
    }

    [Fact]
    public async Task ProcessImage_ReturnsLargeAndThumbnail()
    {
        var file = CreateFormFile(800, 600);

        var result = await _service.ProcessImage(file);

        result.LargeImage.Should().NotBeNull();
        result.Thumbmage.Should().NotBeNull();
        result.LargeImage.Length.Should().BeGreaterThan(0);
        result.Thumbmage.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ProcessImage_ResizesLargeImage_WhenExceeds2000Pixels()
    {
        var file = CreateFormFile(3000, 2000);

        var result = await _service.ProcessImage(file);

        result.LargeImage.Position = 0;
        using var image = await Image.LoadAsync(result.LargeImage);
        image.Width.Should().BeLessThanOrEqualTo(2000);
        image.Height.Should().BeLessThanOrEqualTo(2000);
    }

    [Fact]
    public async Task ProcessImage_DoesNotResize_WhenUnder2000Pixels()
    {
        var file = CreateFormFile(800, 600);

        var result = await _service.ProcessImage(file);

        result.LargeImage.Position = 0;
        using var image = await Image.LoadAsync(result.LargeImage);
        image.Width.Should().Be(800);
        image.Height.Should().Be(600);
    }

    [Fact]
    public async Task ProcessImage_ThumbnailIs300x300()
    {
        var file = CreateFormFile(800, 600);

        var result = await _service.ProcessImage(file);

        result.Thumbmage.Position = 0;
        using var image = await Image.LoadAsync(result.Thumbmage);
        image.Width.Should().Be(300);
        image.Height.Should().Be(300);
    }

    [Fact]
    public async Task ProcessImage_ThumbnailIsSquare_EvenForNonSquareInput()
    {
        var file = CreateFormFile(1000, 500);

        var result = await _service.ProcessImage(file);

        result.Thumbmage.Position = 0;
        using var image = await Image.LoadAsync(result.Thumbmage);
        image.Width.Should().Be(300);
        image.Height.Should().Be(300);
    }

    [Fact]
    public async Task ProcessImage_HandlesSquareImage()
    {
        var file = CreateFormFile(500, 500);

        var result = await _service.ProcessImage(file);

        result.Thumbmage.Position = 0;
        using var image = await Image.LoadAsync(result.Thumbmage);
        image.Width.Should().Be(300);
        image.Height.Should().Be(300);
    }

    [Fact]
    public async Task ProcessImage_HandlesSmallImage()
    {
        var file = CreateFormFile(100, 100);

        var result = await _service.ProcessImage(file);

        result.LargeImage.Position = 0;
        using var largeImage = await Image.LoadAsync(result.LargeImage);
        largeImage.Width.Should().Be(100);
        largeImage.Height.Should().Be(100);

        result.Thumbmage.Position = 0;
        using var thumbImage = await Image.LoadAsync(result.Thumbmage);
        thumbImage.Width.Should().Be(300);
        thumbImage.Height.Should().Be(300);
    }
}
