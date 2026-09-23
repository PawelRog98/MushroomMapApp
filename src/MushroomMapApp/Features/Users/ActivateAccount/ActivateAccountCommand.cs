using MediatR;
using Microsoft.EntityFrameworkCore;
using MushroomMapApp.Domain.Data;
using MushroomMapApp.Domain.Enums;
using MushroomMapApp.Domain.Exceptions;

namespace MushroomMapApp.Features.Users.ActivateAccount;

public record ActivateAccountRequest(string Code,  string Email);
public record ActivateAccountCommand(ActivateAccountRequest Request) : IRequest<Unit>;
public class ActivateAccountCommandHandler : IRequestHandler<ActivateAccountCommand, Unit>
{
    private readonly AppDbContext _context;

    public ActivateAccountCommandHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(ActivateAccountCommand request, CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var token = await _context.Tokens.Where(x=> x.TokenTypeValue == TokenType.ActivationToken.ToString()
                                                                      && x.ExpireDateTime > DateTime.UtcNow
                                                                      && x.User.Email == request.Request.Email
                                                                      && x.TokenData == request.Request.Code)
                .OrderByDescending(x=>x.ExpireDateTime)
                .SingleOrDefaultAsync(cancellationToken);

            if (token == null)
                throw new BadAuthenticationException("Token not found");

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == request.Request.Email, cancellationToken);

            user.IsEmailConfirmed = true;

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
