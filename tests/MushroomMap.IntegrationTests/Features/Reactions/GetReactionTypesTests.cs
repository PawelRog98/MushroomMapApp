using System.Net;
using FluentAssertions;
using MushroomMap.IntegrationTests.Fixtures;
using MushroomMap.IntegrationTests.Helpers;
using Xunit;

namespace MushroomMap.IntegrationTests.Features.Reactions;

public class GetReactionTypesTests : IntegrationTestBase
{
    public GetReactionTypesTests(IntegrationTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task GetReactionTypes_ReturnsSeededReactionTypes()
    {
        var client = CreateClient();

        var types = await client.GetReactionTypesAsync();

        types.Should().HaveCount(4);
        types.Select(t => t.Key).Should().BeEquivalentTo(new[] { "like", "dislike", "love", "mushroom" });
        types.Should().OnlyContain(t =>
            t.PublicId != Guid.Empty
            && !string.IsNullOrWhiteSpace(t.Name)
            && !string.IsNullOrWhiteSpace(t.Icon));
    }

    [Fact]
    public async Task GetReactionTypes_WithoutToken_IsAvailableAnonymously()
    {
        var client = CreateClient();

        var response = await client.GetAsync("/api/reactions/types");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.ReadApiAsync<object>();
        content.Success.Should().BeTrue();
    }
}
