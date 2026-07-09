namespace MushroomMapApp.Infrastructure.Services.FileStorage;

public class ProcessedImage : IAsyncDisposable
{
    public required MemoryStream LargeImage { get; init; }
    public required MemoryStream Thumbmage { get; init; }
    public async ValueTask DisposeAsync()
    {
        await LargeImage.DisposeAsync();
        await Thumbmage.DisposeAsync();
    }
}
