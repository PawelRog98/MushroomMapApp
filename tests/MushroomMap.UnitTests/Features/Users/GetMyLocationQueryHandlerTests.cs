using FluentAssertions;
using MushroomMap.UnitTests.Common.Database;
using MushroomMapApp.Domain.Data;
using MushroomMapApp.Domain.Entities;
using MushroomMapApp.Features.Users.GetMyLocation;
using Xunit;
using Point = NetTopologySuite.Geometries.Point;

namespace MushroomMap.UnitTests.Features.Users;

public class GetMyLocationQueryHandlerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly GetMyLocationQueryHandler _handler;
    private readonly User _user;

    public GetMyLocationQueryHandlerTests()
    {
        _context = TestDbContextFactory.Create();
        _handler = new GetMyLocationQueryHandler(_context);

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

    [Fact]
    public async Task Handle_ReturnsNull_WhenUserHasNoSavedLocation()
    {
        await _context.SaveChangesAsync();

        var result = await _handler.Handle(new GetMyLocationQuery(_user.Id), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ReturnsNull_WhenUserDoesNotExist()
    {
        await _context.SaveChangesAsync();

        var result = await _handler.Handle(new GetMyLocationQuery(12345), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ReturnsLocation_WhenItExists()
    {
        var publicId = Guid.NewGuid();
        _context.UserSavedLocations.Add(new UserSavedLocation
        {
            PublicId = publicId,
            UserId = _user.Id,
            LocationName = "My location",
            Coordinates = new Point(21.0122, 52.2297),
            CreatedAtUtc = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        var result = await _handler.Handle(new GetMyLocationQuery(_user.Id), CancellationToken.None);

        result.Should().NotBeNull();
        result!.PublicId.Should().Be(publicId);
        result.LocationName.Should().Be("My location");
        result.Lat.Should().Be(52.2297);
        result.Lng.Should().Be(21.0122);
    }

    [Fact]
    public async Task Handle_ReturnsOnlyRequestedUsersLocation()
    {
        var otherUser = new User
        {
            PublicId = Guid.NewGuid(),
            Email = "other@test.com",
            PublicNick = "other",
            FirstName = "other",
            LastName = "other",
            PasswordHash = "hash"
        };
        _context.Users.Add(otherUser);
        await _context.SaveChangesAsync();

        _context.UserSavedLocations.Add(new UserSavedLocation
        {
            PublicId = Guid.NewGuid(),
            UserId = otherUser.Id,
            LocationName = "My location",
            Coordinates = new Point(19.9366, 50.0614),
            CreatedAtUtc = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        var result = await _handler.Handle(new GetMyLocationQuery(_user.Id), CancellationToken.None);

        result.Should().BeNull();
    }
}
