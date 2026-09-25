using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using MushroomMap.UnitTests.Common.Database;
using MushroomMapApp.Domain.Data;
using MushroomMapApp.Domain.Entities;
using MushroomMapApp.Domain.Enums;
using MushroomMapApp.Domain.Exceptions;
using MushroomMapApp.Domain.Interfaces;
using MushroomMapApp.Features.Locations.UpdateLocation;
using MushroomMapApp.Infrastructure.Services.FileStorage;
using NetTopologySuite.Geometries;
using Xunit;
using Location = MushroomMapApp.Domain.Entities.Location;

namespace MushroomMap.UnitTests.Features.Locations;

public class UpdateLocationCommandHandlerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<IFileStorage> _fileStorageMock;
    private readonly Mock<IImageProcessingService> _imageProcessingServiceMock;
    private readonly Mock<IFilePathGenerator> _filePathGeneratorMock;
    private readonly UpdateLocationCommandHandler _handler;
    private readonly User _owner;
    private readonly User _otherUser;
    private readonly Location _location;
    private readonly FileResource _keptImage;
    private readonly FileResource _keptThumbnail;
    private readonly FileResource _removedImage;
    private readonly FileResource _removedThumbnail;

    public UpdateLocationCommandHandlerTests()
    {
        _context = TestDbContextFactory.Create();
        _fileStorageMock = new Mock<IFileStorage>();
        _imageProcessingServiceMock = new Mock<IImageProcessingService>();
        _filePathGeneratorMock = new Mock<IFilePathGenerator>();

        _filePathGeneratorMock
            .Setup(x => x.GenerateFilePath("jpg", null, null))
            .Returns("locations/updated-large.jpg");
        _filePathGeneratorMock
            .Setup(x => x.GenerateFilePath("jpg", "_thumb", "thumbnails"))
            .Returns("locations/updated-thumb.jpg");

        _imageProcessingServiceMock
            .Setup(x => x.ProcessImage(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()))
            .Returns(() => Task.FromResult(new ProcessedImage
            {
                LargeImage = new MemoryStream([1, 2, 3]),
                Thumbmage = new MemoryStream([4, 5, 6])
            }));

        _handler = new UpdateLocationCommandHandler(
            _context,
            _fileStorageMock.Object,
            _imageProcessingServiceMock.Object,
            _filePathGeneratorMock.Object);

        _owner = new User
        {
            PublicId = Guid.NewGuid(),
            Email = "owner@test.com",
            PublicNick = "owner",
            FirstName = "test",
            LastName = "test",
            PasswordHash = "hash"
        };

        _otherUser = new User
        {
            PublicId = Guid.NewGuid(),
            Email = "other@test.com",
            PublicNick = "other",
            FirstName = "test",
            LastName = "test",
            PasswordHash = "hash"
        };

        _context.Users.AddRange(_owner, _otherUser);

        _location = new Location
        {
            PublicId = Guid.NewGuid(),
            Name = "Old name",
            Text = "Old text",
            Coordinates = new Point(19.9366, 50.0614),
            CreatedBy = _owner
        };

        _context.Locations.Add(_location);
        _context.SaveChanges();

        _keptImage = new FileResource
        {
            PublicId = Guid.NewGuid(),
            FileName = "images/kept.jpg",
            ContentType = "image/jpeg",
            Size = 1,
            CreatedAtUtc = DateTime.UtcNow,
            TypeEnum = FileType.Image,
            LocationId = _location.Id
        };

        _keptThumbnail = new FileResource
        {
            PublicId = Guid.NewGuid(),
            FileName = "images/kept_thumb.jpg",
            ContentType = "image/jpeg",
            Size = 1,
            CreatedAtUtc = DateTime.UtcNow,
            TypeEnum = FileType.Thumbnail,
            LocationId = _location.Id,
            ParentFileResource = _keptImage
        };

        _removedImage = new FileResource
        {
            PublicId = Guid.NewGuid(),
            FileName = "images/removed.jpg",
            ContentType = "image/jpeg",
            Size = 1,
            CreatedAtUtc = DateTime.UtcNow,
            TypeEnum = FileType.Image,
            LocationId = _location.Id
        };

        _removedThumbnail = new FileResource
        {
            PublicId = Guid.NewGuid(),
            FileName = "images/removed_thumb.jpg",
            ContentType = "image/jpeg",
            Size = 1,
            CreatedAtUtc = DateTime.UtcNow,
            TypeEnum = FileType.Thumbnail,
            LocationId = _location.Id,
            ParentFileResource = _removedImage
        };

        _context.FileResources.AddRange(
            _keptImage,
            _keptThumbnail,
            _removedImage,
            _removedThumbnail);
        _context.SaveChanges();
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
    public async Task Handle_UpdatesLocationAndReplacesImages_WhenUserIsOwner()
    {
        var request = new UpdateLocationRequest(
            Name: "New name",
            Text: "New text",
            Images: new FormFileCollection { CreateImage().Object },
            KeepImageIds: [_keptImage.PublicId]);

        var result = await _handler.Handle(
            new UpdateLocationCommand(_location.PublicId, request, _owner.Id),
            CancellationToken.None);

        var location = _context.Locations
            .Include(x => x.FileResources)
            .ThenInclude(x => x.Variant)
            .Single();

        location.Name.Should().Be("New name");
        location.Text.Should().Be("New text");
        location.UpdatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));

        result.PublicId.Should().Be(location.PublicId);
        result.Name.Should().Be("New name");
        result.Text.Should().Be("New text");
        result.Lat.Should().Be(50.0614);
        result.Lng.Should().Be(19.9366);

        var files = _context.FileResources.ToList();
        files.Should().HaveCount(4);
        files.Select(x => x.FileName).Should().Contain("images/kept.jpg");
        files.Select(x => x.FileName).Should().Contain("images/kept_thumb.jpg");
        files.Select(x => x.FileName).Should().NotContain("images/removed.jpg");
        files.Select(x => x.FileName).Should().NotContain("images/removed_thumb.jpg");
        files.Select(x => x.FileName).Should().Contain("locations/updated-large.jpg");
        files.Select(x => x.FileName).Should().Contain("locations/updated-thumb.jpg");

        _fileStorageMock.Verify(
            x => x.UploadFile(It.IsAny<Stream>(), "locations/updated-large.jpg", It.IsAny<CancellationToken>()),
            Times.Once);
        _fileStorageMock.Verify(
            x => x.UploadFile(It.IsAny<Stream>(), "locations/updated-thumb.jpg", It.IsAny<CancellationToken>()),
            Times.Once);
        _fileStorageMock.Verify(
            x => x.DeleteFile("images/removed.jpg", It.IsAny<CancellationToken>()),
            Times.Once);
        _fileStorageMock.Verify(
            x => x.DeleteFile("images/removed_thumb.jpg", It.IsAny<CancellationToken>()),
            Times.Once);
        _fileStorageMock.Verify(
            x => x.DeleteFile("images/kept.jpg", It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_Throws_WhenLocationDoesNotExist()
    {
        var request = new UpdateLocationRequest(
            Name: "New name",
            Text: "New text",
            Images: new FormFileCollection(),
            KeepImageIds: []);

        var act = () => _handler.Handle(
            new UpdateLocationCommand(Guid.NewGuid(), request, _owner.Id),
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        _location.Name.Should().Be("Old name");
    }

    [Fact]
    public async Task Handle_Throws_WhenUserIsNotOwner()
    {
        var request = new UpdateLocationRequest(
            Name: "New name",
            Text: "New text",
            Images: new FormFileCollection { CreateImage().Object },
            KeepImageIds: [_keptImage.PublicId]);

        var act = () => _handler.Handle(
            new UpdateLocationCommand(_location.PublicId, request, _otherUser.Id),
            CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();

        _location.Name.Should().Be("Old name");
        _location.Text.Should().Be("Old text");
        _context.FileResources.Should().HaveCount(4);

        _imageProcessingServiceMock.Verify(
            x => x.ProcessImage(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _fileStorageMock.Verify(
            x => x.UploadFile(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _fileStorageMock.Verify(
            x => x.DeleteFile(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
