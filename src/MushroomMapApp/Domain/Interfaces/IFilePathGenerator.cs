namespace MushroomMapApp.Domain.Interfaces;

public interface IFilePathGenerator
{
    string GenerateFilePath(string extension, string? suffix = null, string? subFolder = null);
}
