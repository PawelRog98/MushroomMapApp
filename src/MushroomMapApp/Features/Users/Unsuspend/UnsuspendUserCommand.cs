using MediatR;
using Microsoft.EntityFrameworkCore;
using MushroomMapApp.Domain.Data;
using MushroomMapApp.Domain.Enums;
using MushroomMapApp.Domain.Exceptions;

namespace MushroomMapApp.Features.Users.Unsuspend;

public record UnsuspendUserRequest(Guid UserPublicId);
public record UnsuspendUserCommand(UnsuspendUserRequest Request) : IRequest<Unit>;

public class UnsuspendUserCommandHandler : IRequestHandler<UnsuspendUserCommand, Unit>
{
    private readonly AppDbContext _context;

    public UnsuspendUserCommandHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(UnsuspendUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.PublicId == request.Request.UserPublicId, cancellationToken);

        if (user == null)
            throw new BadRequestException("Invalid user data.");

        var suspensions = await _context.Suspensions
            .Where(x => x.UserId == user.Id && x.Status == SuspensionStatusEnum.Active)
            .ToListAsync(cancellationToken);

        foreach (var suspension in suspensions)
            suspension.Status = SuspensionStatusEnum.Lifted;

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
