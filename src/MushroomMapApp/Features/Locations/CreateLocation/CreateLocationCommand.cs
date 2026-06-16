using MediatR;
using Microsoft.EntityFrameworkCore;
using MushroomMapApp.Domain.Data;
using MushroomMapApp.Domain.Exceptions;
using MushroomMapApp.Domain.Interfaces;
using NetTopologySuite.Geometries;
using Location = MushroomMapApp.Domain.Entities.Location;

namespace MushroomMapApp.Features.Locations.CreateLocation;

public record CreateLocationRequest(string Name, string Text, double Lat, double Lng);

public record CreateLocationCommand(CreateLocationRequest request, long userId) : IRequest<LocationDto>;

public class CreateLocationCommandHandler : IRequestHandler<CreateLocationCommand, LocationDto>
{
    private readonly AppDbContext _context;

    public  CreateLocationCommandHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<LocationDto> Handle(CreateLocationCommand command, CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
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
            throw;
        }
    }
}
