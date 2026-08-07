using MediatR;
using Microsoft.EntityFrameworkCore;
using MushroomMapApp.Domain.Data;
using MushroomMapApp.Domain.Entities;
using MushroomMapApp.Domain.Enums;
using MushroomMapApp.Domain.Interfaces;
using NetTopologySuite.Geometries;
using Location = MushroomMapApp.Domain.Entities.Location;

namespace MushroomMapApp.Features.Locations.GetLocations;

public record GetLocationRequest(string? search, double south, double west, double north, double east) : IRequest<(IEnumerable<LocationListItemDto> Items, Dictionary<Guid, LocationPermissionResult> Permissions)>;
public record  GetLocationQuery(GetLocationRequest request) : IRequest<(IEnumerable<LocationListItemDto> Items, Dictionary<Guid, LocationPermissionResult> Permissions)>;

public class GetLocationQueryHandler : IRequestHandler<GetLocationQuery, (IEnumerable<LocationListItemDto> Items, Dictionary<Guid, LocationPermissionResult> Permissions)>
{
    private readonly AppDbContext _context;
    private readonly IResourcePermissionEvaluator<Location, LocationPermissionContext, LocationPermissionResult> _evaluator;
    private readonly IPermissionsContextFactory<LocationPermissionContext> _contextFactory;

    public GetLocationQueryHandler(AppDbContext context,  IResourcePermissionEvaluator<Location, LocationPermissionContext, LocationPermissionResult> evaluator, IPermissionsContextFactory<LocationPermissionContext> contextFactory)
    {
        _context = context;
        _evaluator = evaluator;
        _contextFactory = contextFactory;
    }

    public async Task<(IEnumerable<LocationListItemDto> Items, Dictionary<Guid, LocationPermissionResult> Permissions)> Handle(GetLocationQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);

            var query = _context.Locations
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.request.search))
            {
                query = query.Where(x => x.Name.Contains(request.request.search));
            }

            var envelope = new Envelope(request.request.west,
                request.request.east,
                request.request.south,
                request.request.north);

            var polygon = geometryFactory.ToGeometry(envelope);

            var locations = await query.Where(x => x.Coordinates.Intersects(polygon))
                .Include(x=>x.FileResources)
                .ThenInclude(x=>x.Variant)
                .ToListAsync(cancellationToken);

            var context = await _contextFactory.Create(cancellationToken);
            var permissions = await _evaluator.Evaluate(locations, context, cancellationToken);

            var items = locations
                .Select(x => new LocationListItemDto
                {
                    PublicId = x.PublicId,
                    Name = x.Name,
                    Text = x.Text,
                    Lat = x.Coordinates.Y,
                    Lng = x.Coordinates.X,
                    Images = x.FileResources
                        .Where(y=>y.Type == FileType.Image.ToString())
                        .Select(y=> new ImageDto
                        {
                            PublicId = y.PublicId,
                            ThumbnailUrl = y.Variant
                                .Where(z=>z.Type == FileType.Thumbnail.ToString())
                                .Select(z=>$"{z.FileName}")
                                .FirstOrDefault(),
                            ContentType = y.ContentType
                        }).ToList()
                })
                .ToList();

            return (items, permissions);

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }
}
