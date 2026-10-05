using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MushroomMap.IntegrationTests.Fixtures;
using MushroomMap.IntegrationTests.Helpers;
using MushroomMapApp.Features.Reactions.GetReactionsForLocation;
using Xunit;

namespace MushroomMap.IntegrationTests.Features.Reactions;

public class AddReactionTests : IntegrationTestBase
{
    public AddReactionTests(IntegrationTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task AddReaction_ForNewReaction_AddsReactionToLocation()
    {
        var user = await CreateUserAsync();
        var location = await user.Client.CreateLocationAsync();
        var like = (await user.Client.GetReactionTypesAsync()).Single(t => t.Key == "like");

        var response = await user.Client.PostAsJsonAsync("/api/reactions/add-reaction",
            new { locationPublicId = location.PublicId, reactionTypePublicId = like.PublicId });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.ReadApiAsync<object>();
        content.Success.Should().BeTrue();

        var reactions = await ReadReactionsAsync(user.Client, location.PublicId);
        var reaction = reactions.Single();
        reaction.Key.Should().Be("like");
        reaction.Count.Should().Be(1);
        reaction.HasReacted.Should().BeTrue();
    }

    [Fact]
    public async Task AddReaction_ForDifferentReaction_ReplacesExistingReaction()
    {
        var user = await CreateUserAsync();
        var location = await user.Client.CreateLocationAsync();
        var types = await user.Client.GetReactionTypesAsync();
        var like = types.Single(t => t.Key == "like");
        var mushroom = types.Single(t => t.Key == "mushroom");

        await user.Client.PostAsJsonAsync("/api/reactions/add-reaction",
            new { locationPublicId = location.PublicId, reactionTypePublicId = like.PublicId });

        var response = await user.Client.PostAsJsonAsync("/api/reactions/add-reaction",
            new { locationPublicId = location.PublicId, reactionTypePublicId = mushroom.PublicId });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var reactions = await ReadReactionsAsync(user.Client, location.PublicId);
        var reaction = reactions.Single();
        reaction.Key.Should().Be("mushroom");
        reaction.Count.Should().Be(1);
        reaction.HasReacted.Should().BeTrue();
    }

    [Fact]
    public async Task AddReaction_ForSameReaction_RemovesReaction()
    {
        var user = await CreateUserAsync();
        var location = await user.Client.CreateLocationAsync();
        var like = (await user.Client.GetReactionTypesAsync()).Single(t => t.Key == "like");

        await user.Client.PostAsJsonAsync("/api/reactions/add-reaction",
            new { locationPublicId = location.PublicId, reactionTypePublicId = like.PublicId });

        var response = await user.Client.PostAsJsonAsync("/api/reactions/add-reaction",
            new { locationPublicId = location.PublicId, reactionTypePublicId = like.PublicId });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var reactions = await ReadReactionsAsync(user.Client, location.PublicId);
        reactions.Should().BeEmpty();
    }

    [Fact]
    public async Task AddReaction_WithUnknownLocation_ReturnsBadRequest()
    {
        var user = await CreateUserAsync();
        var like = (await user.Client.GetReactionTypesAsync()).Single(t => t.Key == "like");

        var response = await user.Client.PostAsJsonAsync("/api/reactions/add-reaction",
            new { locationPublicId = Guid.NewGuid(), reactionTypePublicId = like.PublicId });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.ReadApiAsync<object>();
        content.Message.Should().Be("Location not found.");
    }

    [Fact]
    public async Task AddReaction_WithUnknownReactionType_ReturnsBadRequest()
    {
        var user = await CreateUserAsync();
        var location = await user.Client.CreateLocationAsync();

        var response = await user.Client.PostAsJsonAsync("/api/reactions/add-reaction",
            new { locationPublicId = location.PublicId, reactionTypePublicId = Guid.NewGuid() });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.ReadApiAsync<object>();
        content.Message.Should().Be("Reaction type not found.");
    }

    [Fact]
    public async Task AddReaction_WithoutToken_ReturnsUnauthorized()
    {
        var client = CreateClient();

        var response = await client.PostAsJsonAsync("/api/reactions/add-reaction",
            new { locationPublicId = Guid.NewGuid(), reactionTypePublicId = Guid.NewGuid() });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private async Task<List<ReactionDto>> ReadReactionsAsync(HttpClient client, Guid locationPublicId)
    {
        var response = await client.GetAsync($"/api/reactions/get-reactions/{locationPublicId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.ReadApiAsync<List<ReactionDto>>();
        content.Success.Should().BeTrue();

        return content.Data!;
    }
}
