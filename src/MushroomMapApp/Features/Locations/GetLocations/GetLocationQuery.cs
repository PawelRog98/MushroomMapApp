using MediatR;
using Microsoft.EntityFrameworkCore;
using MushroomMapApp.Domain.Data;
using MushroomMapApp.Domain.Entities;
using MushroomMapApp.Domain.Enums;
using MushroomMapApp.Domain.Interfaces;
using MushroomMapApp.Shared.Response;
using NetTopologySuite.Geometries;
using Location = MushroomMapApp.Domain.Entities.Location;

namespace MushroomMapApp.Features.Locations.GetLocations;

public record GetLocationRequest(string? search, double south, double west, double north, double east, int zoom = MapGrid.DefaultZoom);
public record GetLocationQuery(GetLocationRequest request) : IRequest<MapLocationsResponse>;

public class GetLocationQueryHandler : IRequestHandler<GetLocationQuery, MapLocationsResponse>
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

    public async Task<MapLocationsResponse> Handle(GetLocationQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var zoom = MapGrid.ClampZoom(request.request.zoom);

            var query = BuildQuery(request.request);
            var totalCount = await query.CountAsync(cancellationToken);

            var shouldCluster = zoom < MapGrid.ClusterZoomThreshold ||
                                totalCount > MapGrid.MaxMarkers;


            return shouldCluster
                ? await BuildClusters(query, zoom, totalCount, cancellationToken)
                : await BuildMarkers(query, totalCount, cancellationToken);

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }

    private IQueryable<Location> BuildQuery(GetLocationRequest request)
    {
        var query = _context.Locations
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.search))
            query = query.Where(x => x.Name.Contains(request.search));

        var south = Math.Clamp(request.south, -90, 90);
        var north = Math.Clamp(request.north, -90, 90);
        var geometry = new GeometryFactory(new PrecisionModel(),  4326);

        if (request.west <= request.east)
        {
            var polygon = geometry.ToGeometry(new Envelope(request.west, request.east, south, north));
            return query.Where(x => x.Coordinates.Intersects(polygon));
        }

        //antimeridian splitting
        var westPolygon = geometry.ToGeometry(new Envelope(request.west, 180, south, north));
        var eastPolygon = geometry.ToGeometry(new Envelope(-180, request.east, south,  north));

        return query.Where(x=>x.Coordinates.Intersects(westPolygon) ||  x.Coordinates.Intersects(eastPolygon));
    }

    private async Task<MapLocationsResponse> BuildMarkers(
        IQueryable<Location> query,
        int totalCount,
        CancellationToken cancellationToken)
    {
        var locations = await query
            .Include(x => x.CreatedBy)
            .Include(x => x.FileResources).ThenInclude(x => x.Variant)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ThenBy(x => x.Id)
            .Take(MapGrid.MaxMarkers)
            .ToListAsync(cancellationToken);

        var context = await _contextFactory.Create(cancellationToken);
        var permissions = await _evaluator.Evaluate(locations, context, cancellationToken);

        var markers = locations
            .Select(x => new ItemWithMeta<LocationListItemDto>
            {
                Data = LocationDtoMapper.ToListItemDto(x),
                Meta = permissions[x.PublicId]
            })
            .ToList();

        return new MapLocationsResponse
        {
            Mode = "markers",
            TotalCount = totalCount,
            Truncated = totalCount > markers.Count,
            Markers = markers
        };

    }

    private async Task<MapLocationsResponse> BuildClusters(
        IQueryable<Location> query,
        int zoom,
        int totalCount,
        CancellationToken cancellationToken)
    {
        var cell = MapGrid.CellSizeDegrees(zoom);

        var rawClusters = await query
            .GroupBy(x => new
            {
                CellX = Math.Floor(x.Coordinates.X / cell),
                CellY = Math.Floor(x.Coordinates.Y / cell)
            })
            .Select(y => new
            {
                y.Key.CellX,
                y.Key.CellY,
                Lat = y.Average(x => x.Coordinates.Y),
                Lng = y.Average(x => x.Coordinates.X),
                Count = y.Count(),
                South = y.Min(x => x.Coordinates.Y),
                West = y.Min(x => x.Coordinates.X),
                North = y.Max(x => x.Coordinates.Y),
                East = y.Max(x => x.Coordinates.X)
            })
            .ToListAsync(cancellationToken);

        var clusters = rawClusters
            .Select(x => new LocationClusterDto
            {
                Lat = x.Lat,
                Lng = x.Lng,
                Count = x.Count,
                South = x.South,
                West = x.West,
                North = x.North,
                East = x.East,
                Zoom = zoom,
                CellX = (long)x.CellX,
                CellY = (long)x.CellY
            })
            .ToList();

        var truncated = clusters.Count > MapGrid.MaxClusters;

        return new MapLocationsResponse
        {
            Mode = "clusters",
            TotalCount = totalCount,
            Truncated = truncated,
            Clusters = truncated ? clusters.Take(MapGrid.MaxClusters).ToList() : clusters,
        };
    }
}
