using MediatR;
using Microsoft.EntityFrameworkCore;
using MushroomMapApp.Domain.Data;
using MushroomMapApp.Domain.Entities;
using MushroomMapApp.Domain.Exceptions;
using MushroomMapApp.Features.Locations.CreateLocation;
using Point = NetTopologySuite.Geometries.Point;

namespace MushroomMapApp.Features.Users.CreateUserLocation;


public record CreateUserLocationRequest(double Lat, double Lng, Guid? PublicId);
public record CreateUserLocationCommand(CreateUserLocationRequest Request, long UserId) : IRequest<UserLocationDto>;
public class CreateUserLocationCommandHandler : IRequestHandler<CreateUserLocationCommand, UserLocationDto>
{
    private readonly AppDbContext _context;

    public CreateUserLocationCommandHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<UserLocationDto> Handle(CreateUserLocationCommand command, CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Id == command.UserId, cancellationToken);

            if(user == null)
                throw new BadRequestException("Invalid user data.");

            var point = new Point(command.Request.Lng, command.Request.Lat);
            UserSavedLocation location;

            if (command.Request.PublicId.HasValue)
            {
                location = await _context.UserSavedLocations.FirstOrDefaultAsync(x => x.UserId == command.UserId && x.PublicId == command.Request.PublicId, cancellationToken);

                if (location == null)
                    throw new BadRequestException("Invalid location data.");

                location.Coordinates = point;
                location.UpdatedAtUtc = DateTime.UtcNow;
            }
            else
            {

                location = new UserSavedLocation
                {
                    LocationName = "My location",
                    UserId = user.Id,
                    Coordinates = point,
                    CreatedAtUtc = DateTime.UtcNow,
                };

                await _context.AddAsync(location, cancellationToken);
            }

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            var locationDto = new UserLocationDto
            {
                PublicId = location.PublicId,
                LocationName = location.LocationName,
                Lat = location.Coordinates.Y,
                Lng = location.Coordinates.X
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
