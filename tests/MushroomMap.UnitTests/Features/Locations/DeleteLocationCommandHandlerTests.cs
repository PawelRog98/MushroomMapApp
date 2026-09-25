using FluentAssertions;
using MediatR;
using MushroomMap.UnitTests.Common.Database;
using MushroomMapApp.Domain.Data;
using MushroomMapApp.Domain.Entities;
using MushroomMapApp.Domain.Exceptions;
using MushroomMapApp.Features.Locations.DeleteLocation;
using NetTopologySuite.Geometries;
using Xunit;
using Location = MushroomMapApp.Domain.Entities.Location;

namespace MushroomMap.UnitTests.Features.Locations;

public class DeleteLocationCommandHandlerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly DeleteLocationCommandHandler _handler;
    private readonly User _owner;
    private readonly User _otherUser;
    private readonly Location _location;

    public DeleteLocationCommandHandlerTests()
    {
        _context = TestDbContextFactory.Create();
        _handler = new DeleteLocationCommandHandler(_context);

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

        _location = new Location
        {
            PublicId = Guid.NewGuid(),
            Name = "Old pine",
            Text = "Nice spot",
            Coordinates = new Point(19.9366, 50.0614),
            CreatedBy = _owner
        };

        _context.Users.AddRange(_owner, _otherUser);
        _context.Locations.Add(_location);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task Handle_RemovesLocation_WhenUserIsOwner()
    {
        await _context.SaveChangesAsync();

        var command = new DeleteLocationCommand(PublicId: _location.PublicId, UserId: _owner.Id);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().Be(Unit.Value);
        _context.Locations.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_Throws_WhenLocationDoesNotExist()
    {
        await _context.SaveChangesAsync();

        var command = new DeleteLocationCommand(PublicId: Guid.NewGuid(), UserId: _owner.Id);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        _context.Locations.Should().ContainSingle();
    }

    [Fact]
    public async Task Handle_Throws_WhenUserIsNotOwner()
    {
        await _context.SaveChangesAsync();

        var command = new DeleteLocationCommand(PublicId: _location.PublicId, UserId: _otherUser.Id);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
        _context.Locations.Should().ContainSingle();
    }
}
