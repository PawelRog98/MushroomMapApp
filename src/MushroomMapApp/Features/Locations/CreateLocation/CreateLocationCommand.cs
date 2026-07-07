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

    public  CreateLocationCommandHandler(AppDbContext context, IFileStorage fileStorage)
    {
        _context = context;
        _fileStorage = fileStorage;
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
                var savedPath = await _fileStorage.UploadFile(image);

                var fileResource = new FileResource
                {
                    FileName = savedPath,
                    ContentType = image.ContentType,
                    Size = image.Length,
                    CreatedAtUtc = DateTime.UtcNow,
                    TypeEnum = FileType.Image,
                    LocationId = location.Id
                };

                await _context.FileResources.AddAsync(fileResource, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                savedImagePaths.Add(savedPath);
            }

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
