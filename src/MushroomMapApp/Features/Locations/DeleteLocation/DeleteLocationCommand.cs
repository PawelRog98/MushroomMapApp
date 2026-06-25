using MediatR;
using Microsoft.EntityFrameworkCore;
using MushroomMapApp.Domain.Data;
using MushroomMapApp.Domain.Exceptions;

namespace MushroomMapApp.Features.Locations.DeleteLocation;

public record DeleteLocationCommand(Guid PublicId, long UserId) : IRequest<Unit>;

public class DeleteLocationCommandHandler : IRequestHandler<DeleteLocationCommand, Unit>
{
    private readonly AppDbContext _context;

    public DeleteLocationCommandHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteLocationCommand command, CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var location = await _context.Locations
                .FirstOrDefaultAsync(x => x.PublicId == command.PublicId, cancellationToken);

            if (location == null)
                throw new NotFoundException("Location not found.");

            if (location.CreatedById != command.UserId)
                throw new ForbiddenException("You do not have permission to delete this location.");

            _context.Locations.Remove(location);
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
