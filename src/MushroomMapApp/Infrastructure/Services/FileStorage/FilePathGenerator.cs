using MushroomMapApp.Domain.Interfaces;

namespace MushroomMapApp.Infrastructure.Services.FileStorage;

public class FilePathGenerator : IFilePathGenerator
{
    public string GenerateFilePath(string extension, string? suffix = null)
    {
        var now = DateTime.UtcNow;

        var folder = Path.Combine(
            now.Year.ToString(),
            now.Month.ToString("D2"),
            now.Day.ToString("D2"));

        var baseName = Guid.NewGuid().ToString();

        var filename = $"{Guid.NewGuid()}{suffix}.{extension}";

        return Path.Combine(folder, filename);
    }
}
