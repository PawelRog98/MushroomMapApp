using Microsoft.Extensions.Options;
using MushroomMapApp.Domain.Interfaces;
using SixLabors.ImageSharp;

namespace MushroomMapApp.Infrastructure.Services.FileStorage;

public class FileManager : IFileStorage
{
    private readonly string _storagePath;
    public FileManager(IOptions<FileStorageOptions> options)
    {
        _storagePath = options.Value.RootPath;

        if (string.IsNullOrWhiteSpace(_storagePath))
            throw new InvalidOperationException("File storage root path is not configured.");

        Directory.CreateDirectory(_storagePath);
    }


    public async Task UploadFile(Stream fileStream, string relativePath, CancellationToken cancellationToken = default)
    {
        var fullPath = GetFullPath(relativePath);

        var directory = Path.GetDirectoryName(fullPath);

        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        await using var stream = new FileStream(fullPath, FileMode.Create);
        await fileStream.CopyToAsync(stream, cancellationToken);
    }

    public async Task<FileStream?> DownloadFile(string relativePath, CancellationToken cancellationToken = default)
    {
        var fullPath = GetFullPath(relativePath);

        await Task.CompletedTask;
        if (!File.Exists(fullPath))
            throw new FileNotFoundException("File does not exist");

        return new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
    }

    public async Task DeleteFile(string relativePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return;

        var fullPath = GetFullPath(relativePath);

        if (File.Exists(fullPath))
            File.Delete(fullPath);

        await Task.CompletedTask;
    }

    private string GetFullPath(string relativePath)
    {
        relativePath = relativePath.Replace('/', Path.DirectorySeparatorChar);
        return Path.Combine(_storagePath, relativePath);
    }
}
