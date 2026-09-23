using System.Security.Cryptography;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MushroomMapApp.Domain.Data;
using MushroomMapApp.Domain.Entities;
using MushroomMapApp.Domain.Enums;
using MushroomMapApp.Domain.Exceptions;
using MushroomMapApp.Features.Events;

namespace MushroomMapApp.Features.Users.CreateNewVerificationToken;


public record CreateNewVerificationTokenRequest(string Email);
public record CreateNewVerificationTokenCommand(CreateNewVerificationTokenRequest Request) : IRequest<Unit>;
public class CreateNewVerificationTokenCommandHandler : IRequestHandler<CreateNewVerificationTokenCommand, Unit>
{
    private readonly AppDbContext _context;
    private readonly IMediator _mediator;

    public CreateNewVerificationTokenCommandHandler(AppDbContext context, IMediator mediator)
    {
        _context = context;
        _mediator = mediator;
    }

    public async Task<Unit> Handle(CreateNewVerificationTokenCommand request, CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == request.Request.Email, cancellationToken);

            if (user == null)
                throw new NotFoundException("User not found.");

            var verificationToken = new Token
            {
                UserId = user.Id,
                TokenData = Convert.ToHexString(RandomNumberGenerator.GetBytes(5)),
                ExpireDateTime = DateTime.UtcNow.AddHours(3),
                TokenType = TokenType.ActivationToken
            };

            _context.Tokens.Add(verificationToken);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            await _mediator.Publish(new UserRegisteredEvent(user.Email, verificationToken.TokenData),
                cancellationToken);

            return Unit.Value;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
