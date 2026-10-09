using FluentAssertions;
using Moq;
using MushroomMap.UnitTests.Common.Database;
using MushroomMapApp.Domain.Data;
using MushroomMapApp.Domain.Entities;
using MushroomMapApp.Domain.Enums;
using MushroomMapApp.Domain.Interfaces;
using MushroomMapApp.Features.Files.DownloadImage;
using Xunit;

namespace MushroomMap.UnitTests.Features.Files;

public class GetImageQueryHandlerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<IFileStorage> _fileStorageMock;
    private readonly GetImageQueryHandler _handler;

    public GetImageQueryHandlerTests()
    {
        _context = TestDbContextFactory.Create();
        _fileStorageMock = new Mock<IFileStorage>();
        _handler = new GetImageQueryHandler(_context, _fileStorageMock.Object);
    }

    public void Dispose() => _context.Dispose();

    private async Task<FileResource> SeedImageAsync(string fileName = "image.jpg")
    {
        var image = new FileResource
        {
            PublicId = Guid.NewGuid(),
            FileName = fileName,
            ContentType = "image/jpeg",
            Type = FileType.Image.ToString()
        };
        _context.FileResources.Add(image);
        await _context.SaveChangesAsync();
        return image;
    }

    [Fact]
    public async Task Handle_ReturnsNull_WhenImageNotFound()
    {
        var query = new GetImageQuery(Guid.NewGuid());

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ReturnsNull_WhenFileNotFoundOnDisk()
    {
        var image = await SeedImageAsync();

        _fileStorageMock
            .Setup(x => x.DownloadFile(image.FileName, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FileNotFoundException());

        var query = new GetImageQuery(image.PublicId);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ReturnsImageResult_WhenImageExists()
    {
        var image = await SeedImageAsync();
        var imageBytes = new byte[] { 1, 2, 3, 4, 5 };
        var tempFile = Path.GetTempFileName();
        File.WriteAllBytes(tempFile, imageBytes);

        _fileStorageMock
            .Setup(x => x.DownloadFile(image.FileName, It.IsAny<CancellationToken>()))
            .Returns(() => Task.FromResult<FileStream?>(new FileStream(tempFile, FileMode.Open, FileAccess.Read)));

        var query = new GetImageQuery(image.PublicId);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result!.FileBytes.Should().BeEquivalentTo(imageBytes);
        result.ContentType.Should().Be("image/jpeg");
        result.FileName.Should().Be("image.jpg");

        File.Delete(tempFile);
    }

    [Fact]
    public async Task Handle_ReturnsNull_WhenFileStorageReturnsNull()
    {
        var image = await SeedImageAsync();

        _fileStorageMock
            .Setup(x => x.DownloadFile(image.FileName, It.IsAny<CancellationToken>()))
            .ReturnsAsync((FileStream?)null);

        var query = new GetImageQuery(image.PublicId);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_DoesNotReturnThumbnail_WhenQueryingByThumbnailId()
    {
        var image = await SeedImageAsync("image.jpg");
        var thumbnail = new FileResource
        {
            PublicId = Guid.NewGuid(),
            FileName = "thumb.jpg",
            ContentType = "image/jpeg",
            Type = FileType.Thumbnail.ToString(),
            ParentFileResource = image
        };
        _context.FileResources.Add(thumbnail);
        await _context.SaveChangesAsync();

        var query = new GetImageQuery(thumbnail.PublicId);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeNull();
    }
}
