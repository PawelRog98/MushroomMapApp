using FluentAssertions;
using MushroomMap.UnitTests.Common.Database;
using MushroomMapApp.Domain.Data;
using MushroomMapApp.Domain.Entities;
using MushroomMapApp.Domain.Exceptions;
using MushroomMapApp.Features.Reactions.GetReactionsForLocation;
using Xunit;

namespace MushroomMap.UnitTests.Features.Reactions;

public class GetReactionsForLocationCommandHandlerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly GetReactionsForLocationCommandHandler _handler;

    public GetReactionsForLocationCommandHandlerTests()
    {
        _context = TestDbContextFactory.Create();
        _handler = new GetReactionsForLocationCommandHandler(_context);
    }

    public void Dispose() => _context.Dispose();

    private async Task<(Location location, User user, ReactionType reactionType)> SeedDataAsync()
    {
        var user = new User
        {
            PublicId = Guid.NewGuid(),
            PublicNick = "tester",
            FirstName = "Test",
            LastName = "User",
            Email = "test@test.com",
            PasswordHash = "hash"
        };
        _context.Users.Add(user);

        var reactionType = new ReactionType
        {
            PublicId = Guid.NewGuid(),
            Key = "like",
            Name = "Like",
            Icon = "👍"
        };
        _context.ReactionTypes.Add(reactionType);

        var location = new Location
        {
            PublicId = Guid.NewGuid(),
            Name = "Test Location",
            Text = "Test",
            Coordinates = new NetTopologySuite.Geometries.Point(19.0, 50.0),
            CreatedById = user.Id
        };
        _context.Locations.Add(location);

        await _context.SaveChangesAsync();
        return (location, user, reactionType);
    }

    [Fact]
    public async Task Handle_ThrowsBadRequest_WhenLocationNotFound()
    {
        var command = new GetReactionsForLocationCommand(
            new GetReactionsForLocationRequest(Guid.NewGuid()), null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("Location not found.");
    }

    [Fact]
    public async Task Handle_ReturnsReactionCounts_GroupedByType()
    {
        var (location, user, reactionType) = await SeedDataAsync();

        var user2 = new User
        {
            PublicId = Guid.NewGuid(),
            PublicNick = "tester2",
            FirstName = "Test2",
            LastName = "User2",
            Email = "test2@test.com",
            PasswordHash = "hash"
        };
        _context.Users.Add(user2);
        await _context.SaveChangesAsync();

        _context.Reactions.AddRange(
            new Reaction { LocationId = location.Id, UserId = user.Id, ReactionTypeId = reactionType.Id },
            new Reaction { LocationId = location.Id, UserId = user2.Id, ReactionTypeId = reactionType.Id });
        await _context.SaveChangesAsync();

        var command = new GetReactionsForLocationCommand(
            new GetReactionsForLocationRequest(location.PublicId), null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().ContainSingle();
        result.First().PublicId.Should().Be(reactionType.PublicId);
        result.First().Key.Should().Be("like");
        result.First().Name.Should().Be("Like");
        result.First().Icon.Should().Be("👍");
        result.First().Count.Should().Be(2);
        result.First().HasReacted.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ReturnsHasReacted_WhenCurrentUserReacted()
    {
        var (location, user, reactionType) = await SeedDataAsync();

        _context.Reactions.Add(new Reaction
        {
            LocationId = location.Id,
            UserId = user.Id,
            ReactionTypeId = reactionType.Id
        });
        await _context.SaveChangesAsync();

        var command = new GetReactionsForLocationCommand(
            new GetReactionsForLocationRequest(location.PublicId), user.Id);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().ContainSingle();
        result.First().HasReacted.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ReturnsHasReactedFalse_WhenCurrentUserDidNotReact()
    {
        var (location, user, reactionType) = await SeedDataAsync();

        var otherUser = new User
        {
            PublicId = Guid.NewGuid(),
            PublicNick = "other",
            FirstName = "Other",
            LastName = "User",
            Email = "other@test.com",
            PasswordHash = "hash"
        };
        _context.Users.Add(otherUser);
        await _context.SaveChangesAsync();

        _context.Reactions.Add(new Reaction
        {
            LocationId = location.Id,
            UserId = otherUser.Id,
            ReactionTypeId = reactionType.Id
        });
        await _context.SaveChangesAsync();

        var command = new GetReactionsForLocationCommand(
            new GetReactionsForLocationRequest(location.PublicId), user.Id);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().ContainSingle();
        result.First().HasReacted.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ReturnsMultipleReactionTypes()
    {
        var (location, user, reactionType) = await SeedDataAsync();

        var user2 = new User
        {
            PublicId = Guid.NewGuid(),
            PublicNick = "tester2",
            FirstName = "Test2",
            LastName = "User2",
            Email = "test2@test.com",
            PasswordHash = "hash"
        };
        _context.Users.Add(user2);

        var reactionType2 = new ReactionType
        {
            PublicId = Guid.NewGuid(),
            Key = "love",
            Name = "Love",
            Icon = "❤️"
        };
        _context.ReactionTypes.Add(reactionType2);
        await _context.SaveChangesAsync();

        _context.Reactions.AddRange(
            new Reaction { LocationId = location.Id, UserId = user.Id, ReactionTypeId = reactionType.Id },
            new Reaction { LocationId = location.Id, UserId = user2.Id, ReactionTypeId = reactionType2.Id });
        await _context.SaveChangesAsync();

        var command = new GetReactionsForLocationCommand(
            new GetReactionsForLocationRequest(location.PublicId), null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_ReturnsEmpty_WhenNoReactions()
    {
        var (location, _, _) = await SeedDataAsync();

        var command = new GetReactionsForLocationCommand(
            new GetReactionsForLocationRequest(location.PublicId), null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeEmpty();
    }
}
