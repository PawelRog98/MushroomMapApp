using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MushroomMapApp.Domain.Data;
using MushroomMapApp.Domain.Entities;
using MushroomMapApp.Domain.Enums;
using MushroomMapApp.Domain.Exceptions;
using MushroomMapApp.Shared.Response;
using System.Security.Cryptography;

namespace MushroomMapApp.Features.Users.Register;

public record RegisterRequest(
    string Email,
    string PublicNick,
    string FirstName,
    string LastName,
    string Password,
    string ConfirmPassword,
    DateTime? DateOfBirth);

public record Command(RegisterRequest Register) : IRequest<Unit>;

public class Handler : IRequestHandler<Command, Unit>
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;

    public Handler(AppDbContext context, IPasswordHasher<User> passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<Unit> Handle(Command command, CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var existedUser = await _context.Users.AnyAsync(u => u.Email == command.Register.Email, cancellationToken);

            if (existedUser)
                throw new BadRequestException("A user with such an email already exists.");

            var defaultRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User", cancellationToken);
            if (defaultRole == null)
            {
                defaultRole = new Role { Name = "User" };
                _context.Roles.Add(defaultRole);
                await _context.SaveChangesAsync(cancellationToken);
            }

            var user = new User
            {
                Email = command.Register.Email,
                PublicNick = command.Register.PublicNick,
                FirstName = command.Register.FirstName,
                LastName = command.Register.LastName,
                DateOfBirth = DateTime.SpecifyKind(command.Register.DateOfBirth!.Value, DateTimeKind.Utc),
                RoleId = defaultRole.Id,
                IsEmailConfirmed = false
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, command.Register.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync(cancellationToken);

            var verificationToken = new Token
            {
                UserId = user.Id,
                TokenData = Convert.ToHexString(RandomNumberGenerator.GetBytes(64)),
                ExpireDateTime = DateTime.UtcNow.AddHours(3),
                TokenType = TokenType.ActivationToken
            };

            _context.Tokens.Add(verificationToken);
            await _context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return Unit.Value;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
