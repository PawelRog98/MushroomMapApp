using MediatR;
using Microsoft.EntityFrameworkCore;
using MushroomMapApp.Domain.Data;
using MushroomMapApp.Domain.Entities;
using NetTopologySuite.Geometries;
using Location = MushroomMapApp.Domain.Entities.Location;

namespace MushroomMapApp.Features.Locations.GetLocations;

public record GetLocationRequest(string? search, double south, double west, double north, double east) : IRequest<IEnumerable<LocationListItemDto>>;
public record  GetLocationQuery(GetLocationRequest request) : IRequest<IEnumerable<LocationListItemDto>>;

public class GetLocationQueryHandler : IRequestHandler<GetLocationQuery, IEnumerable<LocationListItemDto>>
{
    private readonly AppDbContext _context;

    public GetLocationQueryHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<LocationListItemDto>> Handle(GetLocationQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);

            var query = _context.Locations.AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.request.search))
            {
                query = query.Where(x => x.Name.Contains(request.request.search));
            }

            var envelope = new Envelope(request.request.west,
                request.request.east,
                request.request.south,
                request.request.north);

            var polygon = geometryFactory.ToGeometry(envelope);

            return await query
                .Where(x => x.Coordinates.Intersects(polygon))
                .Select(x => new LocationListItemDto
                {
                    PublicId = x.PublicId,
                    Name = x.Name,
                    Text = x.Text,
                    Lat = x.Coordinates.Y,
                    Lng = x.Coordinates.X
                })
                .ToListAsync(cancellationToken);

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }
}
