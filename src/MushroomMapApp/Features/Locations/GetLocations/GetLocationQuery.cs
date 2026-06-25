using LinqKit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MushroomMapApp.Domain.Data;
using MushroomMapApp.Domain.Entities;

namespace MushroomMapApp.Features.Locations.GetLocations;

public record GetLocationRequest(string? search);
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
            var predicate = PredicateBuilder.New<Location>(true);

            if (!string.IsNullOrWhiteSpace(request.request.search))
            {
                predicate = predicate.And(x=>x.Name.Contains(request.request.search));
            }

            return await _context.Locations
                .AsExpandable()
                .Where(predicate)
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
