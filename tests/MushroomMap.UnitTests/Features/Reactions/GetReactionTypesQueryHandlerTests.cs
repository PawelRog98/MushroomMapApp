using FluentAssertions;
using MushroomMap.UnitTests.Common.Database;
using MushroomMapApp.Domain.Data;
using MushroomMapApp.Domain.Entities;
using MushroomMapApp.Features.Reactions.GetReactionTypes;
using Xunit;

namespace MushroomMap.UnitTests.Features.Reactions;

public class GetReactionTypesQueryHandlerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly GetReactionTypesQueryHandler _handler;

    public GetReactionTypesQueryHandlerTests()
    {
        _context = TestDbContextFactory.Create();
        _handler = new GetReactionTypesQueryHandler(_context);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task Handle_ReturnsAllReactionTypes()
    {
        _context.ReactionTypes.AddRange(
            new ReactionType { PublicId = Guid.NewGuid(), Key = "like", Name = "Like", Icon = "👍" },
            new ReactionType { PublicId = Guid.NewGuid(), Key = "dislike", Name = "Dislike", Icon = "👎" },
            new ReactionType { PublicId = Guid.NewGuid(), Key = "love", Name = "Love", Icon = "❤️" });
        await _context.SaveChangesAsync();

        var result = await _handler.Handle(new GetReactionTypesQuery(), CancellationToken.None);

        result.Should().HaveCount(3);
        result.Select(x => x.Key).Should().BeEquivalentTo("like", "dislike", "love");
    }

    [Fact]
    public async Task Handle_ReturnsMappedDto()
    {
        var publicId = Guid.NewGuid();
        _context.ReactionTypes.Add(new ReactionType
        {
            PublicId = publicId,
            Key = "mushroom",
            Name = "Mushroom",
            Icon = "🍄"
        });
        await _context.SaveChangesAsync();

        var result = await _handler.Handle(new GetReactionTypesQuery(), CancellationToken.None);

        result.Should().ContainSingle();
        result.First().PublicId.Should().Be(publicId);
        result.First().Key.Should().Be("mushroom");
        result.First().Name.Should().Be("Mushroom");
        result.First().Icon.Should().Be("🍄");
    }

    [Fact]
    public async Task Handle_ReturnsEmpty_WhenNoReactionTypes()
    {
        var result = await _handler.Handle(new GetReactionTypesQuery(), CancellationToken.None);

        result.Should().BeEmpty();
    }
}
