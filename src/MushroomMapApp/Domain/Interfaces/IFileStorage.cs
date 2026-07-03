namespace MushroomMapApp.Domain.Interfaces;

public interface IFileStorage
{
    Task<string> UploadFile(IFormFile file);
    Task<FileStream?> DownloadFile(string fileName);
    Task DeleteFile(string fileName);
}
