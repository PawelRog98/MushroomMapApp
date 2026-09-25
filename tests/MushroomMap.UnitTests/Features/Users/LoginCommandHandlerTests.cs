using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using MushroomMap.UnitTests.Common.Database;
using MushroomMapApp.Domain.Data;
using MushroomMapApp.Domain.Entities;
using MushroomMapApp.Domain.Interfaces;
using MushroomMapApp.Domain.Models;
using MushroomMapApp.Features.Users.Login;
using Xunit;

namespace MushroomMap.UnitTests.Features.Users;

public class LoginCommandHandlerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly PasswordHasher<User> _passwordHasher;
    private readonly Handler _handler;

    public LoginCommandHandlerTests()
    {
        _context = TestDbContextFactory.Create();
        _authServiceMock = new Mock<IAuthService>();
        _passwordHasher = new PasswordHasher<User>();

        _handler = new Handler(
            _context,
            _authServiceMock.Object,
            _passwordHasher);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task Handle_ReturnCorrectResult_WhenIsValid()
    {
        var email = "test@test.com";
        var password = "Password123!";

        var role = new Role { Name = "User" };

        var user = new User
        {
            PublicId = Guid.NewGuid(),
            Email = email,
            PublicNick = "test",
            FirstName = "test",
            LastName = "test",
            IsEmailConfirmed = true,
            Role = role
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, password);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _authServiceMock
            .Setup(x => x.GenerateJwtToken(
                It.Is<UserModel>(m =>
                    m.Id == user.Id &&
                    m.FirstName == user.FirstName &&
                    m.LastName == user.LastName &&
                    m.RoleName == role.Name &&
                    m.PublicNick == user.PublicNick),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthTokenModel(
                AccessToken: "access-token",
                RefreshToken: "refresh-token",
                UserNick: user.PublicNick));

        var command = new Command(new LoginRequest(Email: email, Password: password));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.AccessToken.Should().Be("access-token");
        result.RefreshToken.Should().Be("refresh-token");
        result.UserNick.Should().Be(user.PublicNick);
        result.UserId.Should().Be(user.PublicId.ToString());

        _authServiceMock.Verify(
            x => x.GenerateJwtToken(
                It.IsAny<UserModel>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
