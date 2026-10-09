using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using MushroomMap.UnitTests.Common.Database;
using MushroomMapApp.Domain.Data;
using MushroomMapApp.Domain.Entities;
using MushroomMapApp.Domain.Enums;
using MushroomMapApp.Domain.Exceptions;
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

    [Fact]
    public async Task Handle_Throws_WhenEmailIsNotConfirmed()
    {
        var email = "inactive@test.com";
        var password = "Password123!";

        var user = new User
        {
            PublicId = Guid.NewGuid(),
            Email = email,
            PublicNick = "inactive",
            FirstName = "test",
            LastName = "test",
            IsEmailConfirmed = false,
            Role = new Role { Name = "User" }
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, password);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var command = new Command(new LoginRequest(email, password));

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotActiveUserException>();
        _authServiceMock.Verify(
            x => x.GenerateJwtToken(It.IsAny<UserModel>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ThrowsSuspendedUserException_WhenUserHasActiveSuspension()
    {
        var email = "suspended@test.com";
        var password = "Password123!";
        var suspendUntil = new DateTime(2026, 12, 31, 23, 59, 59, DateTimeKind.Utc);

        var user = new User
        {
            PublicId = Guid.NewGuid(),
            Email = email,
            PublicNick = "suspended",
            FirstName = "test",
            LastName = "test",
            IsEmailConfirmed = true,
            Role = new Role { Name = "User" }
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, password);

        _context.Users.Add(user);
        _context.Suspensions.Add(new Suspension
        {
            StartDate = DateTime.UtcNow,
            EndDate = suspendUntil,
            Reason = "unit test suspension",
            Status = SuspensionStatusEnum.Active,
            UserId = user.Id,
            SuspendedById = user.Id
        });
        await _context.SaveChangesAsync();

        var command = new Command(new LoginRequest(email, password));

        var act = () => _handler.Handle(command, CancellationToken.None);

        var exception = await act.Should().ThrowAsync<SuspendedUserException>();
        exception.Which.Message.Should().Be("User is suspended until: 2026-12-31 23:59:59");
        _authServiceMock.Verify(
            x => x.GenerateJwtToken(It.IsAny<UserModel>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ReturnsToken_WhenSuspensionHasExpired()
    {
        var email = "expired-suspension@test.com";
        var password = "Password123!";

        var user = new User
        {
            PublicId = Guid.NewGuid(),
            Email = email,
            PublicNick = "expired",
            FirstName = "test",
            LastName = "test",
            IsEmailConfirmed = true,
            Role = new Role { Name = "User" }
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, password);

        _context.Users.Add(user);
        _context.Suspensions.Add(new Suspension
        {
            StartDate = DateTime.UtcNow.AddDays(-10),
            EndDate = DateTime.UtcNow.AddDays(-1),
            Reason = "expired suspension",
            Status = SuspensionStatusEnum.Active,
            UserId = user.Id,
            SuspendedById = user.Id
        });
        await _context.SaveChangesAsync();

        _authServiceMock
            .Setup(x => x.GenerateJwtToken(It.IsAny<UserModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthTokenModel("access-token", "refresh-token", user.PublicNick));

        var command = new Command(new LoginRequest(email, password));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.AccessToken.Should().Be("access-token");
    }

    [Fact]
    public async Task Handle_ReturnsToken_WhenSuspensionWasLifted()
    {
        var email = "lifted@test.com";
        var password = "Password123!";

        var user = new User
        {
            PublicId = Guid.NewGuid(),
            Email = email,
            PublicNick = "lifted",
            FirstName = "test",
            LastName = "test",
            IsEmailConfirmed = true,
            Role = new Role { Name = "User" }
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, password);

        _context.Users.Add(user);
        _context.Suspensions.Add(new Suspension
        {
            StartDate = DateTime.UtcNow.AddDays(-5),
            EndDate = DateTime.UtcNow.AddDays(5),
            Reason = "lifted suspension",
            Status = SuspensionStatusEnum.Lifted,
            UserId = user.Id,
            SuspendedById = user.Id
        });
        await _context.SaveChangesAsync();

        _authServiceMock
            .Setup(x => x.GenerateJwtToken(It.IsAny<UserModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthTokenModel("access-token", "refresh-token", user.PublicNick));

        var command = new Command(new LoginRequest(email, password));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.AccessToken.Should().Be("access-token");
    }
}
