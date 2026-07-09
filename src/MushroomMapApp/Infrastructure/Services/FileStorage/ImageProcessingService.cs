using MushroomMapApp.Domain.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace MushroomMapApp.Infrastructure.Services.FileStorage;

public class ImageProcessingService : IImageProcessingService
{
    public async Task<ProcessedImage> ProcessImage(IFormFile file,
        CancellationToken cancellationToken = default)
    {
        using var source = await Image.LoadAsync(file.OpenReadStream(), cancellationToken);

        var largeStream = new MemoryStream();
        using var largeImage = source.CloneAs<Rgba32>();
        if(largeImage.Width > 2000 || largeImage.Height > 2000)
            largeImage.Mutate(x=>x.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Max,
                Size = new Size(2000,2000)
            }));

        await largeImage.SaveAsync(largeStream, new JpegEncoder { Quality = 80 }, cancellationToken);
        largeStream.Position = 0;

        var thumbStream = new MemoryStream();

        using var thumbImage = source.CloneAs<Rgba32>();

        var minDim = Math.Min(thumbImage.Height, thumbImage.Width);
        thumbImage.Mutate(x =>
        {
            var cropRect = new Rectangle(
                (thumbImage.Width - minDim) / 2,
                (thumbImage.Height - minDim) / 2,
                minDim, minDim);
            x.Crop(cropRect);
            x.Resize(300, 300);
        });

        await thumbImage.SaveAsync(thumbStream, new JpegEncoder { Quality = 70 }, cancellationToken);
        thumbStream.Position = 0;

        return new ProcessedImage
        {
            LargeImage = largeStream,
            Thumbmage =  thumbStream
        };
    }
}
