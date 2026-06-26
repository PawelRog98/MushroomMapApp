using MediatR;
using Microsoft.EntityFrameworkCore;
using MushroomMapApp.Domain.Data;

namespace MushroomMapApp.Features.Reactions.GetReactionTypes;

public record GetReactionTypesQuery : IRequest<IEnumerable<ReactionTypeDto>>;

public class GetReactionTypesQueryHandler : IRequestHandler<GetReactionTypesQuery, IEnumerable<ReactionTypeDto>>
{
    private readonly AppDbContext _context;

    public GetReactionTypesQueryHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ReactionTypeDto>> Handle(GetReactionTypesQuery request, CancellationToken cancellationToken)
    {
        return await _context.ReactionTypes
            .Select(x => new ReactionTypeDto
            {
                PublicId = x.PublicId,
                Key = x.Key,
                Name = x.Name,
                Icon = x.Icon,
            })
            .ToListAsync(cancellationToken);
    }
}
