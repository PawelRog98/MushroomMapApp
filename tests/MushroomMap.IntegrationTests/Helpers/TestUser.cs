namespace MushroomMap.IntegrationTests.Helpers;

public sealed class TestUser
{
    public required string Email { get; init; }
    public required string Nick { get; init; }
    public required Guid PublicId { get; init; }
    public required string AccessToken { get; init; }
    public required HttpClient Client { get; init; }
}
