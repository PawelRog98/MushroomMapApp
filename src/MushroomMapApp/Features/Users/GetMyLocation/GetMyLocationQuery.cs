using MediatR;
using Microsoft.EntityFrameworkCore;
using MushroomMapApp.Domain.Data;
using MushroomMapApp.Features.Users.CreateUserLocation;

namespace MushroomMapApp.Features.Users.GetMyLocation;

public record GetMyLocationQuery(long UserId) : IRequest<UserLocationDto?>;

public class GetMyLocationQueryHandler : IRequestHandler<GetMyLocationQuery, UserLocationDto?>
{
    private readonly AppDbContext _context;

    public GetMyLocationQueryHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<UserLocationDto?> Handle(GetMyLocationQuery query, CancellationToken cancellationToken)
    {
        var location = await _context.UserSavedLocations
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == query.UserId, cancellationToken);

        if (location == null)
            return null;

        return new UserLocationDto
        {
            PublicId = location.PublicId,
            LocationName = location.LocationName,
            Lat = location.Coordinates.Y,
            Lng = location.Coordinates.X
        };
    }
}
