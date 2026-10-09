using FluentAssertions;
using MushroomMap.UnitTests.Common.Database;
using MushroomMapApp.Domain.Data;
using MushroomMapApp.Domain.Entities;
using MushroomMapApp.Domain.Enums;
using MushroomMapApp.Domain.Exceptions;
using MushroomMapApp.Features.Users.GetUserData;
using Xunit;

namespace MushroomMap.UnitTests.Features.Users;

public class GetUserDataQueryHandlerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly GetUserDataQueryHandler _handler;

    public GetUserDataQueryHandlerTests()
    {
        _context = TestDbContextFactory.Create();
        _handler = new GetUserDataQueryHandler(_context);
    }

    public void Dispose() => _context.Dispose();

    private async Task<User> SeedUserAsync()
    {
        var role = new Role { Name = "User" };
        _context.Roles.Add(role);
        await _context.SaveChangesAsync();

        var user = new User
        {
            PublicId = Guid.NewGuid(),
            PublicNick = "testnick",
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            DateOfBirth = new DateTime(1990, 1, 1),
            AccountInfo = "Test account",
            IsEmailConfirmed = true,
            RoleId = role.Id
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    [Fact]
    public async Task Handle_ThrowsNotFound_WhenUserDoesNotExist()
    {
        var query = new GetUserDataQuery(new GetUserDataRequest(Guid.NewGuid()));

        var act = () => _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("User not found.");
    }

    [Fact]
    public async Task Handle_ReturnsUserData()
    {
        var user = await SeedUserAsync();

        var query = new GetUserDataQuery(new GetUserDataRequest(user.PublicId));

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.PublicNick.Should().Be("testnick");
        result.UserId.Should().Be(user.PublicId);
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
        result.Email.Should().Be("john@example.com");
        result.DateOfBirth.Should().Be(new DateTime(1990, 1, 1));
        result.AccountInfo.Should().Be("Test account");
        result.IsEmailConfirmed.Should().BeTrue();
        result.RoleName.Should().Be("User");
        result.CreatedAtUtc.Should().Be(user.CreatedAtUtc);
    }

    [Fact]
    public async Task Handle_ReturnsEmptyString_WhenAccountInfoIsNull()
    {
        var user = await SeedUserAsync();
        user.AccountInfo = null;
        await _context.SaveChangesAsync();

        var query = new GetUserDataQuery(new GetUserDataRequest(user.PublicId));

        var result = await _handler.Handle(query, CancellationToken.None);

        result.AccountInfo.Should().Be(string.Empty);
    }

    [Fact]
    public async Task Handle_ReturnsAvatarUrl_WhenAvatarExists()
    {
        var user = await SeedUserAsync();

        var avatar = new FileResource
        {
            PublicId = Guid.NewGuid(),
            FileName = "avatar.jpg",
            ContentType = "image/jpeg",
            Type = FileType.Image.ToString(),
            UserId = user.Id
        };

        var thumbnail = new FileResource
        {
            PublicId = Guid.NewGuid(),
            FileName = "avatar_thumb.jpg",
            ContentType = "image/jpeg",
            Type = FileType.Thumbnail.ToString(),
            ParentFileResource = avatar
        };

        avatar.Variant.Add(thumbnail);

        _context.FileResources.Add(avatar);
        await _context.SaveChangesAsync();

        user.AvatarFileResourceId = avatar.Id;
        await _context.SaveChangesAsync();

        var query = new GetUserDataQuery(new GetUserDataRequest(user.PublicId));

        var result = await _handler.Handle(query, CancellationToken.None);

        result.AvatarUrl.Should().Be("avatar.jpg");
        result.AvatarThumbnailUrl.Should().Be("avatar_thumb.jpg");
    }

    [Fact]
    public async Task Handle_ReturnsNullAvatar_WhenNoAvatar()
    {
        var user = await SeedUserAsync();

        var query = new GetUserDataQuery(new GetUserDataRequest(user.PublicId));

        var result = await _handler.Handle(query, CancellationToken.None);

        result.AvatarUrl.Should().BeNull();
        result.AvatarThumbnailUrl.Should().BeNull();
    }
}
