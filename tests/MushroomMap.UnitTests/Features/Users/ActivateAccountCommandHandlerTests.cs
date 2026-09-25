using FluentAssertions;
using MediatR;
using MushroomMap.UnitTests.Common.Database;
using MushroomMapApp.Domain.Data;
using MushroomMapApp.Domain.Entities;
using MushroomMapApp.Domain.Enums;
using MushroomMapApp.Domain.Exceptions;
using MushroomMapApp.Features.Users.ActivateAccount;
using Xunit;

namespace MushroomMap.UnitTests.Features.Users;

public class ActivateAccountCommandHandlerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly ActivateAccountCommandHandler _handler;

    public ActivateAccountCommandHandlerTests()
    {
        _context = TestDbContextFactory.Create();
        _handler = new ActivateAccountCommandHandler(_context);
    }

    public void Dispose() => _context.Dispose();

    private User AddUser(bool isEmailConfirmed = false)
    {
        var user = new User
        {
            PublicId = Guid.NewGuid(),
            Email = "test@test.com",
            PublicNick = "test",
            FirstName = "test",
            LastName = "test",
            PasswordHash = "hash",
            IsEmailConfirmed = isEmailConfirmed
        };

        _context.Users.Add(user);
        return user;
    }

    [Fact]
    public async Task Handle_ConfirmsEmail_WhenTokenIsValid()
    {
        var user = AddUser();
        _context.Tokens.Add(new Token
        {
            User = user,
            TokenData = "CODE123",
            ExpireDateTime = DateTime.UtcNow.AddHours(1),
            TokenType = TokenType.ActivationToken
        });
        await _context.SaveChangesAsync();

        var command = new ActivateAccountCommand(
            new ActivateAccountRequest(Code: "CODE123", Email: user.Email));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().Be(Unit.Value);
        _context.Users.Single().IsEmailConfirmed.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_Throws_WhenCodeDoesNotMatch()
    {
        var user = AddUser();
        _context.Tokens.Add(new Token
        {
            User = user,
            TokenData = "CODE123",
            ExpireDateTime = DateTime.UtcNow.AddHours(1),
            TokenType = TokenType.ActivationToken
        });
        await _context.SaveChangesAsync();

        var command = new ActivateAccountCommand(
            new ActivateAccountRequest(Code: "WRONG", Email: user.Email));

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BadAuthenticationException>();
        _context.Users.Single().IsEmailConfirmed.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_Throws_WhenTokenIsExpired()
    {
        var user = AddUser();
        _context.Tokens.Add(new Token
        {
            User = user,
            TokenData = "CODE123",
            ExpireDateTime = DateTime.UtcNow.AddMinutes(-1),
            TokenType = TokenType.ActivationToken
        });
        await _context.SaveChangesAsync();

        var command = new ActivateAccountCommand(
            new ActivateAccountRequest(Code: "CODE123", Email: user.Email));

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BadAuthenticationException>();
        _context.Users.Single().IsEmailConfirmed.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_Throws_WhenUserHasNoToken()
    {
        var user = AddUser();
        await _context.SaveChangesAsync();

        var command = new ActivateAccountCommand(
            new ActivateAccountRequest(Code: "CODE123", Email: user.Email));

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BadAuthenticationException>();
    }
}
