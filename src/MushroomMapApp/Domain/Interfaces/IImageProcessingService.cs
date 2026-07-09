using MushroomMapApp.Infrastructure.Services.FileStorage;

namespace MushroomMapApp.Domain.Interfaces;

public interface IImageProcessingService
{
    Task<ProcessedImage> ProcessImage(IFormFile file,
        CancellationToken cancellationToken = default);
}
