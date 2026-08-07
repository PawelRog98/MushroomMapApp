using MediatR;
using Microsoft.EntityFrameworkCore;
using MushroomMapApp.Domain.Data;
using MushroomMapApp.Domain.Entities;
using MushroomMapApp.Domain.Enums;
using MushroomMapApp.Domain.Exceptions;
using MushroomMapApp.Domain.Interfaces;
using MushroomMapApp.Features.Locations.CreateLocation;
using NetTopologySuite.Geometries;

namespace MushroomMapApp.Features.Locations.UpdateLocation;

public record UpdateLocationRequest(string Name, string Text, IFormFileCollection Images, List<Guid> KeepImageIds);

public record UpdateLocationCommand(Guid PublicId, UpdateLocationRequest Request, long UserId) : IRequest<LocationDto>;

public class UpdateLocationCommandHandler : IRequestHandler<UpdateLocationCommand, LocationDto>
{
    private readonly AppDbContext _context;
    private readonly IFileStorage _fileStorage;
    private readonly IImageProcessingService _imageProcessingService;
    private readonly IFilePathGenerator _filePathGenerator;

    public UpdateLocationCommandHandler(AppDbContext context, IFileStorage fileStorage, IImageProcessingService imageProcessingService, IFilePathGenerator filePathGenerator)
    {
        _context = context;
        _fileStorage = fileStorage;
        _imageProcessingService = imageProcessingService;
        _filePathGenerator = filePathGenerator;
    }

    public async Task<LocationDto> Handle(UpdateLocationCommand command, CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        var savedImagePaths = new List<string>();
        try
        {
            var location = await _context.Locations
                .Include(x=>x.FileResources)
                .ThenInclude(x=>x.Variant)
                .FirstOrDefaultAsync(x => x.PublicId == command.PublicId, cancellationToken);

            if (location == null)
                throw new NotFoundException("Location not found.");

            if (location.CreatedById != command.UserId)
                throw new ForbiddenException("You do not have permission to modify this location.");

            var imagesToRemove = location.FileResources
                .Where(x=>x.TypeEnum == FileType.Image &&
                          !command.Request.KeepImageIds.Contains(x.PublicId))
                .ToList();

            location.Name = command.Request.Name;
            location.Text = command.Request.Text;
            location.UpdatedAtUtc = DateTime.UtcNow;

            var removedPaths = new List<string>();
            foreach (var img in imagesToRemove)
            {
                _context.FileResources.Remove(img);
                removedPaths.Add(img.FileName);
                foreach (var thumb in img.Variant)
                {
                    _context.FileResources.Remove(thumb);
                    removedPaths.Add(thumb.FileName);
                }
            }

            foreach (var image in command.Request.Images)
            {
                var processedImages = await _imageProcessingService.ProcessImage(image, cancellationToken);

                var largePath = _filePathGenerator.GenerateFilePath("jpg");
                var thumbnailPath = _filePathGenerator.GenerateFilePath("jpg", "_thumb", "thumbnails");

                await _fileStorage.UploadFile(processedImages.LargeImage, largePath, cancellationToken);
                await _fileStorage.UploadFile(processedImages.Thumbmage, thumbnailPath, cancellationToken);

                var fileLargeResource = new FileResource
                {
                    FileName = largePath,
                    ContentType = image.ContentType,
                    Size = processedImages.LargeImage.Length,
                    CreatedAtUtc = DateTime.UtcNow,
                    TypeEnum = FileType.Image,
                    LocationId = location.Id
                };

                await _context.FileResources.AddAsync(fileLargeResource, cancellationToken);

                var thumbnailResource = new FileResource
                {
                    FileName = thumbnailPath,
                    ContentType = image.ContentType,
                    Size = processedImages.Thumbmage.Length,
                    CreatedAtUtc = DateTime.UtcNow,
                    TypeEnum = FileType.Thumbnail,
                    LocationId = location.Id,
                    ParentFileResource = fileLargeResource
                };

                await _context.FileResources.AddAsync(thumbnailResource, cancellationToken);
                savedImagePaths.Add(largePath);
                savedImagePaths.Add(thumbnailPath);
            }
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            foreach (var removedPath in removedPaths)
            {
                await  _fileStorage.DeleteFile(removedPath, cancellationToken);
            }

            return new LocationDto
            {
                PublicId = location.PublicId,
                Name = location.Name,
                Text = location.Text,
                Lat = location.Coordinates.Y,
                Lng = location.Coordinates.X
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            foreach (var savedImage in savedImagePaths)
            {
                await _fileStorage.DeleteFile(savedImage, cancellationToken);
            }
            throw;
        }
    }
}
