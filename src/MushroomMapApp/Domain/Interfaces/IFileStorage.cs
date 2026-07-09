namespace MushroomMapApp.Domain.Interfaces;

public interface IFileStorage
{
    Task UploadFile(Stream fileStream, string relativePath, CancellationToken cancellationToken = default);
    Task<FileStream?> DownloadFile(string fileName, CancellationToken cancellationToken = default);
    Task DeleteFile(string fileName,  CancellationToken cancellationToken = default);
}
