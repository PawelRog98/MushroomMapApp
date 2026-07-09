using MediatR;
using Microsoft.EntityFrameworkCore;
using MushroomMapApp.Domain.Data;
using MushroomMapApp.Domain.Entities;
using MushroomMapApp.Domain.Enums;
using MushroomMapApp.Domain.Exceptions;
using MushroomMapApp.Domain.Interfaces;
using NetTopologySuite.Geometries;
using Location = MushroomMapApp.Domain.Entities.Location;

namespace MushroomMapApp.Features.Locations.CreateLocation;

public record CreateLocationCommand(CreateLocationRequest request, long userId) : IRequest<LocationDto>;

public class CreateLocationCommandHandler : IRequestHandler<CreateLocationCommand, LocationDto>
{
    private readonly AppDbContext _context;
    private readonly IFileStorage _fileStorage;
    private readonly IImageProcessingService _imageProcessingService;
    private readonly IFilePathGenerator _filePathGenerator;

    public  CreateLocationCommandHandler(AppDbContext context,
        IFileStorage fileStorage,
        IImageProcessingService imageProcessingService,
        IFilePathGenerator filePathGenerator)
    {
        _context = context;
        _fileStorage = fileStorage;
        _imageProcessingService = imageProcessingService;
        _filePathGenerator = filePathGenerator;
    }

    public async Task<LocationDto> Handle(CreateLocationCommand command, CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        var savedImagePaths = new List<string>();

        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x=>x.Id == command.userId, cancellationToken);

            if (user == null)
                throw new BadRequestException("Invalid user data.");

            var point = new Point(command.request.Lng, command.request.Lat);

            var location = new Location
            {
                CreatedById = user.Id,
                Name = command.request.Name,
                Text = command.request.Text,
                Coordinates = point,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            };

            await _context.Locations.AddAsync(location, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            foreach (var image in command.request.Images)
            {
                var processedImages = await _imageProcessingService.ProcessImage(image, cancellationToken);

                var largePath = _filePathGenerator.GenerateFilePath("jpg");
                var thumbnailPath = _filePathGenerator.GenerateFilePath("jpg", "_thumb");

                await _fileStorage.UploadFile(processedImages.LargeImage, largePath, cancellationToken);

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


            var locationDto = new LocationDto
            {
                PublicId = location.PublicId,
                Name = location.Name,
                Text = location.Text,
                Lat = location.Coordinates.Y,
                Lng = location.Coordinates.X,
            };

            return locationDto;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);

            foreach (var savedImage in savedImagePaths)
            {
                await _fileStorage.DeleteFile(savedImage);
            }
            throw;
        }
    }
}
