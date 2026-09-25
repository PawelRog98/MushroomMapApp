using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Moq;
using MushroomMap.UnitTests.Common.Database;
using MushroomMapApp.Domain.Data;
using MushroomMapApp.Domain.Entities;
using MushroomMapApp.Domain.Enums;
using MushroomMapApp.Features.Events;
using MushroomMapApp.Features.Users.Register;
using Xunit;

namespace MushroomMap.UnitTests.Features.Users;

public class RegisterCommandHandlerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly PasswordHasher<User> _passwordHasher;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Handler _handler;

    public RegisterCommandHandlerTests()
    {
        _context = TestDbContextFactory.Create();
        _passwordHasher = new PasswordHasher<User>();
        _mediatorMock = new Mock<IMediator>();

        _handler = new Handler(
            _context,
            _passwordHasher,
            _mediatorMock.Object);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task Handle_ReturnCorrectResult_WhenIsValid()
    {
        var newUser = CreateValidRequest();

        var role = new Role
        {
            Name = "User"
        };

        _context.Roles.Add(role);
        await _context.SaveChangesAsync();

        var command = new Command(newUser);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().Be(Unit.Value);

        _context.Users.Should().ContainSingle();

        var user = _context.Users.Single();

        user.Email.Should().Be(newUser.Email);
        user.PublicNick.Should().Be(newUser.PublicNick);
        user.FirstName.Should().Be(newUser.FirstName);
        user.LastName.Should().Be(newUser.LastName);

        user.PasswordHash.Should().NotBeNullOrEmpty();
        _passwordHasher
            .VerifyHashedPassword(user, user.PasswordHash, newUser.Password)
            .Should().Be(PasswordVerificationResult.Success);

        user.RoleId.Should().Be(role.Id);
        user.IsEmailConfirmed.Should().BeFalse();

        _context.UserRoles.Should().ContainSingle();

        var userRole = _context.UserRoles.Single();

        userRole.UserId.Should().Be(user.Id);
        userRole.RoleId.Should().Be(role.Id);

        _context.Tokens.Should().ContainSingle();

        var token = _context.Tokens.Single();

        token.UserId.Should().Be(user.Id);
        token.TokenType.Should().Be(TokenType.ActivationToken);
        token.TokenData.Should().NotBeNullOrEmpty();

        _mediatorMock.Verify(
            x => x.Publish(
                It.Is<UserRegisteredEvent>(e =>
                    e.Email == newUser.Email &&
                    e.VerificationCode == token.TokenData),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static RegisterRequest CreateValidRequest()
    {
        return new RegisterRequest(
            Email: "test@test.com",
            PublicNick: "test",
            FirstName: "test",
            LastName: "test",
            Password: "Password123!",
            ConfirmPassword: "Password123!",
            DateOfBirth: new DateTime(1995, 5, 10));
    }
}
