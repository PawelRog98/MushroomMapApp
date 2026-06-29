using MediatR;
using Microsoft.EntityFrameworkCore;
using MushroomMapApp.Domain.Data;
using MushroomMapApp.Domain.Entities;
using MushroomMapApp.Domain.Exceptions;

namespace MushroomMapApp.Features.Reactions.GetReactionsForLocation;

public record GetReactionsForLocationRequest(Guid locationPublicId);

public record GetReactionsForLocationCommand(GetReactionsForLocationRequest request, long? userId) : IRequest<IEnumerable<ReactionDto>>;

public class GetReactionsForLocationCommandHandler : IRequestHandler<GetReactionsForLocationCommand, IEnumerable<ReactionDto>>
{
    private readonly AppDbContext _context;

    public GetReactionsForLocationCommandHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ReactionDto>> Handle(GetReactionsForLocationCommand request, CancellationToken cancellationToken)
    {
        var location = await  _context.Locations.FirstOrDefaultAsync(x=>x.PublicId == request.request.locationPublicId, cancellationToken);

        if(location == null)
            throw new BadRequestException("Location not found.");

        Guid? currentUserReaction = null;
        if (request.userId.HasValue)
        {
            currentUserReaction = await _context.Reactions
                .Where(x => x.LocationId == location.Id && x.UserId == request.userId.Value)
                .Select(x => x.ReactionType.PublicId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        return await _context.Reactions
            .Where(x => x.LocationId == location.Id)
            .GroupBy(x => new
            {
                x.ReactionType.PublicId,
                x.ReactionType.Key,
                x.ReactionType.Name,
                x.ReactionType.Icon
            })
            .Select(x => new ReactionDto
            {
                PublicId = x.Key.PublicId,
                Key = x.Key.Key,
                Name = x.Key.Name,
                Icon = x.Key.Icon,
                Count = x.Count(),
                HasReacted = x.Key.PublicId == currentUserReaction
            })
            .ToListAsync(cancellationToken);

    }
}
