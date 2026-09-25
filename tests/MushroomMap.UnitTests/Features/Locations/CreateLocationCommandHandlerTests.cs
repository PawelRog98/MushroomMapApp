using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using MushroomMap.UnitTests.Common.Database;
using MushroomMapApp.Domain.Data;
using MushroomMapApp.Domain.Entities;
using MushroomMapApp.Domain.Enums;
using MushroomMapApp.Domain.Exceptions;
using MushroomMapApp.Domain.Interfaces;
using MushroomMapApp.Features.Locations.CreateLocation;
using MushroomMapApp.Infrastructure.Services.FileStorage;
using Xunit;
using Location = MushroomMapApp.Domain.Entities.Location;

namespace MushroomMap.UnitTests.Features.Locations;

public class CreateLocationCommandHandlerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<IFileStorage> _fileStorageMock;
    private readonly Mock<IImageProcessingService> _imageProcessingServiceMock;
    private readonly Mock<IFilePathGenerator> _filePathGeneratorMock;
    private readonly CreateLocationCommandHandler _handler;
    private readonly User _user;

    public CreateLocationCommandHandlerTests()
    {
        _context = TestDbContextFactory.Create();
        _fileStorageMock = new Mock<IFileStorage>();
        _imageProcessingServiceMock = new Mock<IImageProcessingService>();
        _filePathGeneratorMock = new Mock<IFilePathGenerator>();

        _filePathGeneratorMock
            .Setup(x => x.GenerateFilePath("jpg", null, null))
            .Returns("locations/large.jpg");
        _filePathGeneratorMock
            .Setup(x => x.GenerateFilePath("jpg", "_thumb", "thumbnails"))
            .Returns("locations/thumb.jpg");

        _imageProcessingServiceMock
            .Setup(x => x.ProcessImage(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()))
            .Returns(() => Task.FromResult(new ProcessedImage
            {
                LargeImage = new MemoryStream([1, 2, 3]),
                Thumbmage = new MemoryStream([4, 5, 6])
            }));

        _handler = new CreateLocationCommandHandler(
            _context,
            _fileStorageMock.Object,
            _imageProcessingServiceMock.Object,
            _filePathGeneratorMock.Object);

        _user = new User
        {
            PublicId = Guid.NewGuid(),
            Email = "test@test.com",
            PublicNick = "test",
            FirstName = "test",
            LastName = "test",
            PasswordHash = "hash"
        };

        _context.Users.Add(_user);
    }

    public void Dispose() => _context.Dispose();

    private static Mock<IFormFile> CreateImage()
    {
        var image = new Mock<IFormFile>();
        image.SetupGet(x => x.ContentType).Returns("image/jpeg");
        image.SetupGet(x => x.Length).Returns(1024);
        image.Setup(x => x.OpenReadStream()).Returns(() => new MemoryStream([1]));
        return image;
    }

    [Fact]
    public async Task Handle_CreatesLocationWithoutImages_WhenRequestIsValid()
    {
        await _context.SaveChangesAsync();

        var request = new CreateLocationRequest
        {
            Name = "Old pine",
            Text = "Nice spot",
            Lat = 50.0614,
            Lng = 19.9366,
            Images = new FormFileCollection()
        };

        var result = await _handler.Handle(
            new CreateLocationCommand(request, _user.Id),
            CancellationToken.None);

        result.Name.Should().Be("Old pine");
        result.Text.Should().Be("Nice spot");
        result.Lat.Should().Be(50.0614);
        result.Lng.Should().Be(19.9366);

        var location = _context.Locations.Should().ContainSingle().Which;
        location.CreatedById.Should().Be(_user.Id);
        location.Name.Should().Be("Old pine");
        location.Text.Should().Be("Nice spot");
        location.Coordinates.X.Should().Be(19.9366);
        location.Coordinates.Y.Should().Be(50.0614);
        location.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        result.PublicId.Should().Be(location.PublicId);

        _context.FileResources.Should().BeEmpty();
        _imageProcessingServiceMock.Verify(
            x => x.ProcessImage(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _fileStorageMock.Verify(
            x => x.UploadFile(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_SavesImagesAndThumbnails_WhenRequestContainsImage()
    {
        await _context.SaveChangesAsync();

        var request = new CreateLocationRequest
        {
            Name = "Old pine",
            Text = "Nice spot",
            Lat = 50.0614,
            Lng = 19.9366,
            Images = new FormFileCollection { CreateImage().Object }
        };

        var result = await _handler.Handle(
            new CreateLocationCommand(request, _user.Id),
            CancellationToken.None);

        var location = _context.Locations.Should().ContainSingle().Which;
        result.PublicId.Should().Be(location.PublicId);

        var files = _context.FileResources.ToList();
        files.Should().HaveCount(2);

        var image = files.Single(f => f.TypeEnum == FileType.Image);
        var thumbnail = files.Single(f => f.TypeEnum == FileType.Thumbnail);

        image.LocationId.Should().Be(location.Id);
        image.FileName.Should().Be("locations/large.jpg");
        thumbnail.LocationId.Should().Be(location.Id);
        thumbnail.ParentFileResourceId.Should().Be(image.Id);
        thumbnail.FileName.Should().Be("locations/thumb.jpg");

        _fileStorageMock.Verify(
            x => x.UploadFile(It.IsAny<Stream>(), "locations/large.jpg", It.IsAny<CancellationToken>()),
            Times.Once);
        _fileStorageMock.Verify(
            x => x.UploadFile(It.IsAny<Stream>(), "locations/thumb.jpg", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Throws_WhenUserDoesNotExist()
    {
        await _context.SaveChangesAsync();

        var request = new CreateLocationRequest
        {
            Name = "Old pine",
            Text = "Nice spot",
            Lat = 50.0614,
            Lng = 19.9366,
            Images = new FormFileCollection { CreateImage().Object }
        };

        var act = () => _handler.Handle(
            new CreateLocationCommand(request, userId: 12345),
            CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>();
        _context.Locations.Should().BeEmpty();
        _fileStorageMock.Verify(
            x => x.UploadFile(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _fileStorageMock.Verify(
            x => x.DeleteFile(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
