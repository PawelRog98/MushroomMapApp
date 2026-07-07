using MushroomMapApp.Domain.Interfaces;

namespace MushroomMapApp.Infrastructure.Services;

public class FileManager : IFileStorage
{
    private readonly string _storagePath;
    public FileManager(IConfiguration configuration)
    {
        _storagePath = configuration["FileStorage:RootPath"] ?? "Storage/Files";
        if (!Directory.Exists(_storagePath))
            Directory.CreateDirectory(_storagePath);
    }


    public async Task<string> UploadFile(IFormFile file)
    {
        var dateTimeNow = DateTime.UtcNow;

        var folderPath = Path.Combine(
            _storagePath,
            dateTimeNow.Year.ToString(),
            dateTimeNow.Month.ToString("D2"),
            dateTimeNow.Day.ToString("D2"));

        if(!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        var uniqueName = $"{Guid.NewGuid()}_{dateTimeNow.Year.ToString()}-{dateTimeNow.Month.ToString("D2")}-{dateTimeNow.Day.ToString("D2")}";

        var fullPath = Path.Combine(folderPath, uniqueName);

        using var stream = new FileStream(fullPath, FileMode.Create);
        await  file.CopyToAsync(stream);

        return fullPath;
    }

    public Task<FileStream?> DownloadFile(string fileName)
    {
        if (!File.Exists(fileName))
            throw new FileNotFoundException("File does not exist");

        return Task.FromResult<FileStream?>(new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true));
    }

    public Task DeleteFile(string fileName)
    {
        if (File.Exists(fileName))
            File.Delete(fileName);

        return Task.CompletedTask;
    }
}
