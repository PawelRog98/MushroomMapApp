using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MushroomMap.IntegrationTests.Fixtures;
using MushroomMap.IntegrationTests.Helpers;
using MushroomMapApp.Features.Reactions.GetReactionsForLocation;
using Xunit;

namespace MushroomMap.IntegrationTests.Features.Reactions;

public class GetReactionsForLocationTests : IntegrationTestBase
{
    public GetReactionsForLocationTests(IntegrationTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task GetReactions_ForReactionOfAnotherUser_ReturnsCountWithoutHasReacted()
    {
        var reactor = await CreateUserAsync();
        var viewer = await CreateUserAsync();
        var location = await reactor.Client.CreateLocationAsync();
        var like = (await reactor.Client.GetReactionTypesAsync()).Single(t => t.Key == "like");

        await reactor.Client.PostAsJsonAsync("/api/reactions/add-reaction",
            new { locationPublicId = location.PublicId, reactionTypePublicId = like.PublicId });

        var reactions = await ReadReactionsAsync(viewer.Client, location.PublicId);

        var reaction = reactions.Single();
        reaction.Key.Should().Be("like");
        reaction.Count.Should().Be(1);
        reaction.HasReacted.Should().BeFalse();
    }

    [Fact]
    public async Task GetReactions_ForAnonymousUser_ReturnsCountWithoutHasReacted()
    {
        var user = await CreateUserAsync();
        var location = await user.Client.CreateLocationAsync();
        var like = (await user.Client.GetReactionTypesAsync()).Single(t => t.Key == "like");

        await user.Client.PostAsJsonAsync("/api/reactions/add-reaction",
            new { locationPublicId = location.PublicId, reactionTypePublicId = like.PublicId });

        var anonymous = CreateClient();
        var reactions = await ReadReactionsAsync(anonymous, location.PublicId);

        var reaction = reactions.Single();
        reaction.Count.Should().Be(1);
        reaction.HasReacted.Should().BeFalse();
    }

    [Fact]
    public async Task GetReactions_ForLocationWithoutReactions_ReturnsEmptyList()
    {
        var user = await CreateUserAsync();
        var location = await user.Client.CreateLocationAsync();

        var reactions = await ReadReactionsAsync(user.Client, location.PublicId);

        reactions.Should().BeEmpty();
    }

    [Fact]
    public async Task GetReactions_WithUnknownLocation_ReturnsBadRequest()
    {
        var client = CreateClient();

        var response = await client.GetAsync($"/api/reactions/get-reactions/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.ReadApiAsync<object>();
        content.Success.Should().BeFalse();
        content.Message.Should().Be("Location not found.");
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
