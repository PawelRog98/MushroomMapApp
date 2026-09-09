using MediatR;
using Microsoft.EntityFrameworkCore;
using MushroomMapApp.Domain.Data;

namespace MushroomMapApp.Features.Users.UpdateUserData;

public record UpdateUserDataRequest(string PublicNick, string FirstName, string LastName, DateTime DateOfBirth, string AccountInfo);
public record UpdateUserDataUpdateCommand(UpdateUserDataRequest UpdateUserDataRequest, long UserId) : IRequest<Unit>;
public class UserDataUpdateCommandHandler : IRequestHandler<UpdateUserDataUpdateCommand, Unit>
{
    private readonly AppDbContext _context;

    public  UserDataUpdateCommandHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(UpdateUserDataUpdateCommand request, CancellationToken cancellationToken)
    {
        var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(x=>x.Id == request.UserId, cancellationToken);

            user.FirstName = request.UpdateUserDataRequest.FirstName;
            user.LastName = request.UpdateUserDataRequest.LastName;
            user.DateOfBirth = DateTime.SpecifyKind(request.UpdateUserDataRequest.DateOfBirth, DateTimeKind.Utc);
            user.AccountInfo = request.UpdateUserDataRequest.AccountInfo;
            user.PublicNick = request.UpdateUserDataRequest.PublicNick;


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
