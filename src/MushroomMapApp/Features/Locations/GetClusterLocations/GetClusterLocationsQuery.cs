using MediatR;
using Microsoft.EntityFrameworkCore;
using MushroomMapApp.Domain.Data;
using MushroomMapApp.Domain.Interfaces;
using MushroomMapApp.Features.Locations.GetLocations;
using NetTopologySuite.Geometries;
using Location = MushroomMapApp.Domain.Entities.Location;

namespace MushroomMapApp.Features.Locations.GetClusterLocations;

public record GetClusterLocationsRequest(long cellX, long cellY, int zoom, string? search = null, int page = 1, int pageSize = 25);

public record GetClusterLocationsQuery(GetClusterLocationsRequest request) : IRequest<ClusterLocationsResponse>;

public class GetClusterLocationsQueryHandler : IRequestHandler<GetClusterLocationsQuery, ClusterLocationsResponse>
{
    private readonly AppDbContext _context;
    private readonly IResourcePermissionEvaluator<Location, LocationPermissionContext, LocationPermissionResult> _evaluator;
    private readonly IPermissionsContextFactory<LocationPermissionContext> _contextFactory;

    public GetClusterLocationsQueryHandler(AppDbContext context,
        IResourcePermissionEvaluator<Location, LocationPermissionContext, LocationPermissionResult> evaluator,
        IPermissionsContextFactory<LocationPermissionContext> contextFactory)
    {
        _context = context;
        _evaluator = evaluator;
        _contextFactory = contextFactory;
    }

    public async Task<ClusterLocationsResponse> Handle(GetClusterLocationsQuery request,
        CancellationToken cancellationToken)
    {
        var req = request.request;

        var zoom = MapGrid.ClampZoom(req.zoom);
        var cell = MapGrid.CellSizeDegrees(zoom);

        var cellX = (double)req.cellX;
        var cellY = (double)req.cellY;

        var minLng = req.cellX * cell;
        var maxLng = (req.cellX + 1) * cell;
        var minLat = req.cellY * cell;
        var maxLat = (req.cellY + 1) * cell;

        var geometry = new GeometryFactory(new PrecisionModel(), 4326);
        var envelope = geometry.ToGeometry(new Envelope(minLng, maxLng, minLat,  maxLat));

        var query = _context.Locations
            .AsNoTracking()
            .Where(x => x.Coordinates.Intersects(envelope))
            .Where(x => Math.Floor(x.Coordinates.X / cell) == cellX)
            .Where(x => Math.Floor(x.Coordinates.Y / cell) == cellY);

        if(!string.IsNullOrWhiteSpace(req.search))
            query = query.Where(x=>x.Name.Contains(req.search));

        var totalCount = await query.CountAsync(cancellationToken);

        var totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)req.pageSize));
        var page = Math.Clamp(req.page, 1, totalPages);

        var locations = await query
            .Include(x => x.CreatedBy)
            .Include(x => x.FileResources)
            .ThenInclude(x => x.Variant)
            .AsSplitQuery()
            .OrderByDescending(x => x.CreatedAtUtc)
            .ThenBy(x => x.Id)
            .Skip((page - 1) * req.pageSize)
            .Take(req.pageSize)
            .ToListAsync(cancellationToken);

        var context = await _contextFactory.Create(cancellationToken);
        var permissions = await _evaluator.Evaluate(locations, context, cancellationToken);

        return new ClusterLocationsResponse(
            locations.Select(LocationDtoMapper.ToListItemDto).ToList(),
            permissions,
            totalCount,
            page,
            req.pageSize);
    }
}
