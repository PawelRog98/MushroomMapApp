using MediatR;
using Microsoft.EntityFrameworkCore;
using MushroomMapApp.Domain.Data;
using MushroomMapApp.Domain.Enums;

namespace MushroomMapApp.Features.Users.Unsuspend;

public record UnsuspendExpiredSuspensionsCommand() : IRequest<Unit>;

public class UnsuspendExpiredSuspensionsCommandHandler : IRequestHandler<UnsuspendExpiredSuspensionsCommand, Unit>
{
    private readonly AppDbContext _context;

    public UnsuspendExpiredSuspensionsCommandHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(UnsuspendExpiredSuspensionsCommand request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var suspensions = await _context.Suspensions
            .Where(x => x.Status == SuspensionStatusEnum.Active
                && x.EndDate.HasValue
                && x.EndDate <= now)
            .ToListAsync(cancellationToken);

        foreach (var suspension in suspensions)
            suspension.Status = SuspensionStatusEnum.Expired;

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
