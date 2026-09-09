using MediatR;
using Microsoft.EntityFrameworkCore;
using MushroomMapApp.Domain.Data;
using MushroomMapApp.Domain.Exceptions;

namespace MushroomMapApp.Features.Users.GetUserData;

public record GetUserDataRequest(Guid publicUserId);
public record GetUserDataQuery(GetUserDataRequest Request) : IRequest<UserDataDto>;

public class GetUserDataQueryHandler : IRequestHandler<GetUserDataQuery, UserDataDto>
{
    private readonly AppDbContext _context;
    public GetUserDataQueryHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<UserDataDto> Handle(GetUserDataQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.PublicId == request.Request.publicUserId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        return new UserDataDto
        {
            PublicNick = user.PublicNick,
            UserId = user.PublicId,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            DateOfBirth = user.DateOfBirth,
            AccountInfo = user.AccountInfo ?? string.Empty,
            IsEmailConfirmed = user.IsEmailConfirmed,
            RoleName = user.Role.Name,
            CreatedAtUtc = user.CreatedAtUtc
        };
    }
}
