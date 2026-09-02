using MediatR;
using Microsoft.EntityFrameworkCore;
using MushroomMapApp.Domain.Data;
using MushroomMapApp.Domain.Entities;
using MushroomMapApp.Domain.Enums;
using MushroomMapApp.Domain.Exceptions;

namespace MushroomMapApp.Features.Users.Suspend;

public record SuspendUserRequest(Guid UserPublicId, int Days, string Reason);
public record SuspendUserCommand(long CurrentUserId, SuspendUserRequest Request) : IRequest<Unit>;
public class SuspendUserCommandHandler : IRequestHandler<SuspendUserCommand, Unit>
{
    private readonly AppDbContext _context;
    public SuspendUserCommandHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(SuspendUserCommand request, CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(x=>x.PublicId == request.Request.UserPublicId, cancellationToken);

            if (user == null)
                throw new BadRequestException("Invalid user data.");

            var date = DateTime.UtcNow.AddDays(request.Request.Days);
            var suspensionEndDateFull = new DateTime(date.Year, date.Month, date.Day, 23, 59,  59, 999);

            var suspension = new Suspension
            {
                StartDate = DateTime.UtcNow,
                EndDate = suspensionEndDateFull,
                Reason = request.Request.Reason,
                Status = SuspensionStatusEnum.Active,
                UserId = user.Id,
                SuspendedById = request.CurrentUserId
            };

            await  _context.Suspensions.AddAsync(suspension, cancellationToken);
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
