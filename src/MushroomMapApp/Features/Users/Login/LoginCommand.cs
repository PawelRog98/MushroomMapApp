using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MushroomMapApp.Domain.Data;
using MushroomMapApp.Domain.Entities;
using MushroomMapApp.Domain.Interfaces;
using MushroomMapApp.Domain.Models;
using MushroomMapApp.Domain.Exceptions;
using MushroomMapApp.Shared.Response;

namespace MushroomMapApp.Features.Users.Login;

public record LoginRequest(string Email, string Password);

public record Command(LoginRequest Login) : IRequest<AuthTokenDto>;

public class Handler : IRequestHandler<Command, AuthTokenDto>
{
    private readonly AppDbContext _context;
    private readonly IAuthService _authService;
    private readonly IPasswordHasher<User> _passwordHasher;

    public Handler(AppDbContext context, IAuthService authService, IPasswordHasher<User> passwordHasher)
    {
        _context = context;
        _authService = authService;
        _passwordHasher = passwordHasher;
    }

    public async Task<AuthTokenDto> Handle(Command command, CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == command.Login.Email, cancellationToken);

            if (user == null)
                throw new BadRequestException("Invalid user data.");

            var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, command.Login.Password);
            if (verificationResult == PasswordVerificationResult.Failed)
                throw new BadRequestException("Invalid user data.");

            var userModel = new UserModel(user.Id, user.FirstName, user.LastName, user.Role.Name, user.PublicNick);
            var token = await _authService.GenerateJwtToken(userModel, cancellationToken);

            var result = new AuthTokenDto
            {
                AccessToken = token.AccessToken,
                RefreshToken = token.RefreshToken,
                UserNick = token.UserNick,
                UserId = user.Id.ToString()
            };

            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
