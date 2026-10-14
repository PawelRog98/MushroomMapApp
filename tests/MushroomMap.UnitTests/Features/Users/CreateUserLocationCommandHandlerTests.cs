using FluentAssertions;
using MushroomMap.UnitTests.Common.Database;
using MushroomMapApp.Domain.Data;
using MushroomMapApp.Domain.Entities;
using MushroomMapApp.Domain.Exceptions;
using MushroomMapApp.Features.Users.CreateUserLocation;
using Xunit;
using Point = NetTopologySuite.Geometries.Point;

namespace MushroomMap.UnitTests.Features.Users;

public class CreateUserLocationCommandHandlerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly CreateUserLocationCommandHandler _handler;
    private readonly User _user;

    public CreateUserLocationCommandHandlerTests()
    {
        _context = TestDbContextFactory.Create();
        _handler = new CreateUserLocationCommandHandler(_context);

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

    private async Task<UserSavedLocation> SeedUserLocationAsync(double lat = 52.2297, double lng = 21.0122)
    {
        var location = new UserSavedLocation
        {
            PublicId = Guid.NewGuid(),
            UserId = _user.Id,
            LocationName = "My location",
            Coordinates = new Point(lng, lat),
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.UserSavedLocations.Add(location);
        await _context.SaveChangesAsync();
        return location;
    }

    [Fact]
    public async Task Handle_CreatesLocation_WhenPublicIdIsNull()
    {
        await _context.SaveChangesAsync();

        var command = new CreateUserLocationCommand(
            new CreateUserLocationRequest(50.0614, 19.9366, PublicId: null),
            _user.Id);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.LocationName.Should().Be("My location");
        result.Lat.Should().Be(50.0614);
        result.Lng.Should().Be(19.9366);

        var location = _context.UserSavedLocations.Should().ContainSingle().Which;
        location.UserId.Should().Be(_user.Id);
        location.LocationName.Should().Be("My location");
        location.Coordinates.Y.Should().Be(50.0614);
        location.Coordinates.X.Should().Be(19.9366);
        location.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        location.UpdatedAtUtc.Should().BeNull();
        result.PublicId.Should().Be(location.PublicId);
    }

    [Fact]
    public async Task Handle_UpdatesCoordinates_WhenPublicIdMatches()
    {
        var existing = await SeedUserLocationAsync(lat: 52.2297, lng: 21.0122);

        var command = new CreateUserLocationCommand(
            new CreateUserLocationRequest(50.0614, 19.9366, existing.PublicId),
            _user.Id);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.PublicId.Should().Be(existing.PublicId);
        result.Lat.Should().Be(50.0614);
        result.Lng.Should().Be(19.9366);

        var location = _context.UserSavedLocations.Should().ContainSingle().Which;
        location.Id.Should().Be(existing.Id);
        location.Coordinates.Y.Should().Be(50.0614);
        location.Coordinates.X.Should().Be(19.9366);
        location.UpdatedAtUtc.Should().NotBeNull();
        location.UpdatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task Handle_Throws_WhenPublicIdDoesNotBelongToUser()
    {
        await _context.SaveChangesAsync();

        var command = new CreateUserLocationCommand(
            new CreateUserLocationRequest(50.0614, 19.9366, Guid.NewGuid()),
            _user.Id);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("Invalid location data.");
        _context.UserSavedLocations.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_Throws_WhenUserDoesNotExist()
    {
        await _context.SaveChangesAsync();

        var command = new CreateUserLocationCommand(
            new CreateUserLocationRequest(50.0614, 19.9366, null),
            12345);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("Invalid user data.");
        _context.UserSavedLocations.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_DoesNotUpdateOtherUsersLocations()
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

        var otherLocation = new UserSavedLocation
        {
            PublicId = Guid.NewGuid(),
            UserId = otherUser.Id,
            LocationName = "My location",
            Coordinates = new Point(21.0122, 52.2297),
            CreatedAtUtc = DateTime.UtcNow
        };
        _context.UserSavedLocations.Add(otherLocation);
        await _context.SaveChangesAsync();

        var command = new CreateUserLocationCommand(
            new CreateUserLocationRequest(50.0614, 19.9366, otherLocation.PublicId),
            _user.Id);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>();

        var untouched = _context.UserSavedLocations.Should().ContainSingle().Which;
        untouched.Id.Should().Be(otherLocation.Id);
        untouched.Coordinates.Y.Should().Be(52.2297);
        untouched.Coordinates.X.Should().Be(21.0122);
        untouched.UpdatedAtUtc.Should().BeNull();
    }
}
