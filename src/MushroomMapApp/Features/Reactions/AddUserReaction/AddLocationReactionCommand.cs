using Microsoft.EntityFrameworkCore;
using MediatR;
using MushroomMapApp.Domain.Data;
using MushroomMapApp.Domain.Entities;
using MushroomMapApp.Domain.Exceptions;

namespace MushroomMapApp.Features.Reactions.AddUserReaction;

public record AddReactionRequest(Guid locationPublicId, Guid reactionTypePublicId);

public record AddLocationReactionCommand(AddReactionRequest  request, long userId) : IRequest<Unit>;

public class AddLocationReactionCommandHandler : IRequestHandler<AddLocationReactionCommand, Unit>
{
    private readonly AppDbContext _context;

    public AddLocationReactionCommandHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(AddLocationReactionCommand command, CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x=>x.Id == command.userId, cancellationToken);

            if (user == null)
                throw new BadRequestException("Invalid user data.");

            var location =
                await _context.Locations.FirstOrDefaultAsync(x => x.PublicId == command.request.locationPublicId,
                    cancellationToken);

            if (location == null)
                throw new BadRequestException("Location not found.");

            var reactionType =
                await _context.ReactionTypes.FirstOrDefaultAsync(
                    x => x.PublicId == command.request.reactionTypePublicId, cancellationToken);

            if (reactionType == null)
                throw new BadRequestException("Reaction type not found.");

            var reaction = await _context.Reactions.FirstOrDefaultAsync(x => x.LocationId == location.Id &&
                                                                             x.UserId == user.Id, cancellationToken);
            if (reaction == null)
            {
                var newReaction = new Reaction
                {
                    LocationId = location.Id,
                    UserId = user.Id,
                    ReactionTypeId = reactionType.Id,
                    CreatedAtUtc = DateTime.UtcNow,
                };

                await _context.Reactions.AddAsync(newReaction, cancellationToken);
            }
            else if(reaction.ReactionTypeId != reactionType.Id)
            {
                reaction.ReactionTypeId = reactionType.Id;
            }
            else if(reaction.ReactionTypeId == reactionType.Id)
            {
                _context.Remove(reaction);
            }

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Unit.Value;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
