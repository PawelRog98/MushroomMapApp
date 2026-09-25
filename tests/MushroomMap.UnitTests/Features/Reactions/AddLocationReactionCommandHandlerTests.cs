using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MushroomMap.UnitTests.Common.Database;
using MushroomMapApp.Domain.Data;
using MushroomMapApp.Domain.Entities;
using MushroomMapApp.Domain.Exceptions;
using MushroomMapApp.Features.Reactions.AddUserReaction;
using NetTopologySuite.Geometries;
using Xunit;
using Location = MushroomMapApp.Domain.Entities.Location;

namespace MushroomMap.UnitTests.Features.Reactions;

public class AddLocationReactionCommandHandlerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly AddLocationReactionCommandHandler _handler;
    private readonly User _user;
    private readonly Location _location;
    private readonly ReactionType _likeType;
    private readonly ReactionType _dislikeType;

    public AddLocationReactionCommandHandlerTests()
    {
        _context = TestDbContextFactory.Create();
        _handler = new AddLocationReactionCommandHandler(_context);

        _user = new User
        {
            PublicId = Guid.NewGuid(),
            Email = "test@test.com",
            PublicNick = "test",
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
            CreatedBy = _user
        };

        _likeType = new ReactionType
        {
            PublicId = Guid.NewGuid(),
            Key = "like",
            Name = "Like",
            Icon = "thumb-up"
        };

        _dislikeType = new ReactionType
        {
            PublicId = Guid.NewGuid(),
            Key = "dislike",
            Name = "Dislike",
            Icon = "thumb-down"
        };

        _context.Users.Add(_user);
        _context.Locations.Add(_location);
        _context.ReactionTypes.AddRange(_likeType, _dislikeType);
        _context.SaveChanges();
    }

    public void Dispose() => _context.Dispose();

    private async Task<Reaction?> GetReaction()
    {
        return await _context.Reactions
            .FirstOrDefaultAsync(x => x.LocationId == _location.Id && x.UserId == _user.Id);
    }

    [Fact]
    public async Task Handle_AddsReaction_WhenUserHasNoReactionYet()
    {
        var command = new AddLocationReactionCommand(
            new AddReactionRequest(
                locationPublicId: _location.PublicId,
                reactionTypePublicId: _likeType.PublicId),
            userId: _user.Id);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().Be(Unit.Value);

        var reaction = await GetReaction();
        reaction.Should().NotBeNull();
        reaction!.ReactionTypeId.Should().Be(_likeType.Id);
        reaction.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task Handle_ChangesReactionType_WhenUserPicksAnotherType()
    {
        _context.Reactions.Add(new Reaction
        {
            LocationId = _location.Id,
            UserId = _user.Id,
            ReactionTypeId = _likeType.Id,
            CreatedAtUtc = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        var command = new AddLocationReactionCommand(
            new AddReactionRequest(
                locationPublicId: _location.PublicId,
                reactionTypePublicId: _dislikeType.PublicId),
            userId: _user.Id);

        await _handler.Handle(command, CancellationToken.None);

        var reactions = await _context.Reactions.ToListAsync();
        reactions.Should().ContainSingle();
        reactions.Single().ReactionTypeId.Should().Be(_dislikeType.Id);
    }

    [Fact]
    public async Task Handle_RemovesReaction_WhenUserRepeatsSameType()
    {
        _context.Reactions.Add(new Reaction
        {
            LocationId = _location.Id,
            UserId = _user.Id,
            ReactionTypeId = _likeType.Id,
            CreatedAtUtc = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        var command = new AddLocationReactionCommand(
            new AddReactionRequest(
                locationPublicId: _location.PublicId,
                reactionTypePublicId: _likeType.PublicId),
            userId: _user.Id);

        await _handler.Handle(command, CancellationToken.None);

        (await _context.Reactions.ToListAsync()).Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_Throws_WhenLocationDoesNotExist()
    {
        var command = new AddLocationReactionCommand(
            new AddReactionRequest(
                locationPublicId: Guid.NewGuid(),
                reactionTypePublicId: _likeType.PublicId),
            userId: _user.Id);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("Location not found.");
        (await _context.Reactions.ToListAsync()).Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_Throws_WhenReactionTypeDoesNotExist()
    {
        var command = new AddLocationReactionCommand(
            new AddReactionRequest(
                locationPublicId: _location.PublicId,
                reactionTypePublicId: Guid.NewGuid()),
            userId: _user.Id);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("Reaction type not found.");
        (await _context.Reactions.ToListAsync()).Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_Throws_WhenUserDoesNotExist()
    {
        var command = new AddLocationReactionCommand(
            new AddReactionRequest(
                locationPublicId: _location.PublicId,
                reactionTypePublicId: _likeType.PublicId),
            userId: 12345);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("Invalid user data.");
        (await _context.Reactions.ToListAsync()).Should().BeEmpty();
    }
}
