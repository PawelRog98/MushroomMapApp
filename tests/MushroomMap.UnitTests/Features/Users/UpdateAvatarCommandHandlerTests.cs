using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Moq;
using MushroomMap.UnitTests.Common.Database;
using MushroomMapApp.Domain.Data;
using MushroomMapApp.Domain.Entities;
using MushroomMapApp.Domain.Enums;
using MushroomMapApp.Domain.Exceptions;
using MushroomMapApp.Domain.Interfaces;
using MushroomMapApp.Features.Users.UpdateAvatar;
using MushroomMapApp.Infrastructure.Services.FileStorage;
using Xunit;

namespace MushroomMap.UnitTests.Features.Users;

public class UpdateAvatarCommandHandlerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<IFileStorage> _fileStorageMock;
    private readonly Mock<IImageProcessingService> _imageProcessingServiceMock;
    private readonly Mock<IFilePathGenerator> _filePathGeneratorMock;
    private readonly UpdateAvatarCommandHandler _handler;

    public UpdateAvatarCommandHandlerTests()
    {
        _context = TestDbContextFactory.Create();
        _fileStorageMock = new Mock<IFileStorage>();
        _imageProcessingServiceMock = new Mock<IImageProcessingService>();
        _filePathGeneratorMock = new Mock<IFilePathGenerator>();

        _filePathGeneratorMock
            .Setup(x => x.GenerateFilePath("jpg", null, null))
            .Returns("avatars/large.jpg");
        _filePathGeneratorMock
            .Setup(x => x.GenerateFilePath("jpg", "_thumb", "thumbnails"))
            .Returns("avatars/thumb.jpg");

        _imageProcessingServiceMock
            .Setup(x => x.ProcessImage(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()))
            .Returns(() => Task.FromResult(new ProcessedImage
            {
                LargeImage = new MemoryStream([1, 2, 3]),
                Thumbmage = new MemoryStream([4, 5, 6])
            }));

        _handler = new UpdateAvatarCommandHandler(
            _context,
            _fileStorageMock.Object,
            _imageProcessingServiceMock.Object,
            _filePathGeneratorMock.Object);
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

    private User AddUser()
    {
        var user = new User
        {
            PublicId = Guid.NewGuid(),
            Email = "test@test.com",
            PublicNick = "test",
            FirstName = "test",
            LastName = "test",
            PasswordHash = "hash"
        };

        _context.Users.Add(user);
        return user;
    }

    [Fact]
    public async Task Handle_SavesNewAvatarAndThumbnail_WhenUserHasNoAvatar()
    {
        var user = AddUser();
        await _context.SaveChangesAsync();

        var command = new UpdateAvatarCommand(
            new UpdateAvatarRequest(Image: CreateImage().Object),
            UserId: user.Id);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().Be(Unit.Value);

        var avatar = user.AvatarFileResource;
        avatar.Should().NotBeNull();
        avatar!.FileName.Should().Be("avatars/large.jpg");
        avatar.TypeEnum.Should().Be(FileType.Image);
        avatar.UserId.Should().Be(user.Id);
        avatar.ContentType.Should().Be("image/jpeg");
        user.ModifiedAtUtc.Should().NotBeNull();

        var files = _context.FileResources.ToList();
        files.Should().HaveCount(2);
        files.Should().Contain(f => f.TypeEnum == FileType.Image);
        files.Should().Contain(f =>
            f.TypeEnum == FileType.Thumbnail &&
            f.ParentFileResourceId == avatar.Id &&
            f.FileName == "avatars/thumb.jpg");

        _fileStorageMock.Verify(
            x => x.UploadFile(It.IsAny<Stream>(), "avatars/large.jpg", It.IsAny<CancellationToken>()),
            Times.Once);
        _fileStorageMock.Verify(
            x => x.UploadFile(It.IsAny<Stream>(), "avatars/thumb.jpg", It.IsAny<CancellationToken>()),
            Times.Once);
        _fileStorageMock.Verify(
            x => x.DeleteFile(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ReplacesOldAvatar_WhenUserAlreadyHasAvatar()
    {
        var user = AddUser();

        var oldAvatar = new FileResource
        {
            PublicId = Guid.NewGuid(),
            FileName = "old/large.jpg",
            ContentType = "image/jpeg",
            Size = 1,
            CreatedAtUtc = DateTime.UtcNow,
            TypeEnum = FileType.Image,
            UserId = user.Id
        };

        var oldThumbnail = new FileResource
        {
            PublicId = Guid.NewGuid(),
            FileName = "old/thumb.jpg",
            ContentType = "image/jpeg",
            Size = 1,
            CreatedAtUtc = DateTime.UtcNow,
            TypeEnum = FileType.Thumbnail,
            ParentFileResource = oldAvatar
        };

        user.AvatarFileResource = oldAvatar;
        _context.FileResources.AddRange(oldAvatar, oldThumbnail);
        await _context.SaveChangesAsync();

        var command = new UpdateAvatarCommand(
            new UpdateAvatarRequest(Image: CreateImage().Object),
            UserId: user.Id);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().Be(Unit.Value);

        user.AvatarFileResource.Should().NotBeNull();
        user.AvatarFileResource!.FileName.Should().Be("avatars/large.jpg");

        var files = _context.FileResources.Select(x => x.FileName).ToList();
        files.Should().HaveCount(2);
        files.Should().Contain("avatars/large.jpg");
        files.Should().Contain("avatars/thumb.jpg");
        files.Should().NotContain("old/large.jpg");
        files.Should().NotContain("old/thumb.jpg");

        _fileStorageMock.Verify(
            x => x.DeleteFile("old/large.jpg", It.IsAny<CancellationToken>()),
            Times.Once);
        _fileStorageMock.Verify(
            x => x.DeleteFile("old/thumb.jpg", It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_Throws_WhenUserDoesNotExist()
    {
        var command = new UpdateAvatarCommand(
            new UpdateAvatarRequest(Image: CreateImage().Object),
            UserId: 12345);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        _imageProcessingServiceMock.Verify(
            x => x.ProcessImage(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _fileStorageMock.Verify(
            x => x.UploadFile(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
