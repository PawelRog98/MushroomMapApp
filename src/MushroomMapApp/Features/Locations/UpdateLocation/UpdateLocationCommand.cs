using MediatR;
using Microsoft.EntityFrameworkCore;
using MushroomMapApp.Domain.Data;
using MushroomMapApp.Domain.Exceptions;
using MushroomMapApp.Features.Locations.CreateLocation;
using NetTopologySuite.Geometries;

namespace MushroomMapApp.Features.Locations.UpdateLocation;

public record UpdateLocationRequest(string Name, string Text);

public record UpdateLocationCommand(Guid PublicId, UpdateLocationRequest Request, long UserId) : IRequest<LocationDto>;

public class UpdateLocationCommandHandler : IRequestHandler<UpdateLocationCommand, LocationDto>
{
    private readonly AppDbContext _context;

    public UpdateLocationCommandHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<LocationDto> Handle(UpdateLocationCommand command, CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var location = await _context.Locations
                .FirstOrDefaultAsync(x => x.PublicId == command.PublicId, cancellationToken);

            if (location == null)
                throw new NotFoundException("Location not found.");

            if (location.CreatedById != command.UserId)
                throw new ForbiddenException("You do not have permission to modify this location.");

            location.Name = command.Request.Name;
            location.Text = command.Request.Text;
            location.UpdatedAtUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

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
            throw;
        }
    }
}
