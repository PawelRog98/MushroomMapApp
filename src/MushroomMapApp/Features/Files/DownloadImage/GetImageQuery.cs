using MediatR;
using Microsoft.EntityFrameworkCore;
using MushroomMapApp.Domain.Data;
using MushroomMapApp.Domain.Enums;
using MushroomMapApp.Domain.Interfaces;

namespace MushroomMapApp.Features.Files.DownloadImage;

public record class GetImageQuery(Guid PublicId) : IRequest<ImageResultDto?>;

public class GetImageQueryHandler : IRequestHandler<GetImageQuery, ImageResultDto?>
{
    private readonly AppDbContext _context;
    private readonly IFileStorage _fileStorage;

    public GetImageQueryHandler(AppDbContext context, IFileStorage fileStorage)
    {
        _context = context;
        _fileStorage = fileStorage;
    }

    public async Task<ImageResultDto?> Handle(GetImageQuery request, CancellationToken cancellationToken)
    {
        var fileResource = await _context.FileResources
            .FirstOrDefaultAsync(x => x.PublicId == request.PublicId
                && x.Type == FileType.Image.ToString(), cancellationToken);

        if (fileResource == null)
            return null;

        try
        {
            await using var stream = await _fileStorage.DownloadFile(fileResource.FileName, cancellationToken);
            if (stream is null)
                return null;

            await using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream, cancellationToken);

            return new ImageResultDto
            {
                FileBytes = memoryStream.ToArray(),
                ContentType = fileResource.ContentType,
                FileName = fileResource.FileName
            };
        }
        catch (FileNotFoundException)
        {
            return null;
        }
    }
}
