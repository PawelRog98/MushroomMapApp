using System.IdentityModel.Tokens.Jwt;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MushroomMap.UnitTests.Common.Database;
using MushroomMapApp.Domain.Data;
using MushroomMapApp.Domain.Entities;
using MushroomMapApp.Domain.Enums;
using MushroomMapApp.Domain.Exceptions;
using MushroomMapApp.Domain.Models;
using MushroomMapApp.Infrastructure.Services;
using Xunit;

namespace MushroomMap.UnitTests.Infrastructure.Services;

public class AuthServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly AuthService _authService;
    private readonly JwtSettings _jwtSettings;

    public AuthServiceTests()
    {
        _context = TestDbContextFactory.Create();
        _jwtSettings = new JwtSettings
        {
            JwtKey = "test-jwt-key-with-at-least-32-characters",
            JwtExpireMinutes = 60,
            JwtIssuer = "TestIssuer"
        };
        _authService = new AuthService(_context, _jwtSettings);
    }

    public void Dispose() => _context.Dispose();

    private async Task<User> SeedUserAsync()
    {
        var role = new Role { Name = "User" };
        _context.Roles.Add(role);
        await _context.SaveChangesAsync();

        var user = new User
        {
            PublicId = Guid.NewGuid(),
            PublicNick = "testnick",
            FirstName = "John",
            LastName = "Doe",
            Email = "test@test.com",
            PasswordHash = "hash",
            RoleId = role.Id
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    [Fact]
    public async Task GenerateJwtToken_ReturnsToken_AndSavesRefreshToken()
    {
        var user = await SeedUserAsync();
        var userModel = new UserModel(user.Id, user.FirstName, user.LastName, "User", user.PublicNick);

        var result = await _authService.GenerateJwtToken(userModel, CancellationToken.None);

        result.Should().NotBeNull();
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.UserNick.Should().Be("testnick");

        var savedToken = await _context.Tokens.FirstOrDefaultAsync(t => t.TokenData == result.RefreshToken);
        savedToken.Should().NotBeNull();
        savedToken!.TokenType.Should().Be(TokenType.RefreshToken);
        savedToken.UserId.Should().Be(user.Id);
    }

    [Fact]
    public async Task GenerateJwtToken_ContainsCorrectClaims()
    {
        var user = await SeedUserAsync();
        var userModel = new UserModel(user.Id, user.FirstName, user.LastName, "User", user.PublicNick);

        var result = await _authService.GenerateJwtToken(userModel, CancellationToken.None);

        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(result.AccessToken);

        token.Claims.Should().Contain(c => c.Value == user.Id.ToString());
        token.Claims.Should().Contain(c => c.Value == "John Doe");
        token.Claims.Should().Contain(c => c.Value == "User");
    }

    [Fact]
    public async Task RefreshToken_ThrowsBadRequest_WhenTokenNotFound()
    {
        var act = () => _authService.RefreshToken("non-existent-token", CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("Invalid refresh token.");
    }

    [Fact]
    public async Task RefreshToken_ThrowsBadRequest_WhenTokenIsWrongType()
    {
        var user = await SeedUserAsync();
        _context.Tokens.Add(new Token
        {
            UserId = user.Id,
            TokenData = "activation-token",
            ExpireDateTime = DateTime.UtcNow.AddHours(3),
            TokenType = TokenType.ActivationToken
        });
        await _context.SaveChangesAsync();

        var act = () => _authService.RefreshToken("activation-token", CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("Invalid refresh token.");
    }

    [Fact]
    public async Task RefreshToken_ThrowsBadRequest_WhenTokenExpired()
    {
        var user = await SeedUserAsync();
        _context.Tokens.Add(new Token
        {
            UserId = user.Id,
            TokenData = "expired-token",
            ExpireDateTime = DateTime.UtcNow.AddDays(-1),
            TokenType = TokenType.RefreshToken
        });
        await _context.SaveChangesAsync();

        var act = () => _authService.RefreshToken("expired-token", CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("Refresh token has expired.");
    }

    [Fact]
    public async Task RefreshToken_ReturnsNewToken_AndRemovesOldToken()
    {
        var user = await SeedUserAsync();
        _context.Tokens.Add(new Token
        {
            UserId = user.Id,
            TokenData = "valid-token",
            ExpireDateTime = DateTime.UtcNow.AddDays(30),
            TokenType = TokenType.RefreshToken
        });
        await _context.SaveChangesAsync();

        var result = await _authService.RefreshToken("valid-token", CancellationToken.None);

        result.Should().NotBeNull();
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();

        var oldToken = await _context.Tokens.FirstOrDefaultAsync(t => t.TokenData == "valid-token");
        oldToken.Should().BeNull();
    }

    [Fact]
    public async Task RefreshToken_ThrowsSuspendedUserException_WhenUserSuspended()
    {
        var user = await SeedUserAsync();
        _context.Tokens.Add(new Token
        {
            UserId = user.Id,
            TokenData = "valid-token",
            ExpireDateTime = DateTime.UtcNow.AddDays(30),
            TokenType = TokenType.RefreshToken
        });
        _context.Suspensions.Add(new Suspension
        {
            UserId = user.Id,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(5),
            Reason = "Test suspension",
            Status = SuspensionStatusEnum.Active
        });
        await _context.SaveChangesAsync();

        var act = () => _authService.RefreshToken("valid-token", CancellationToken.None);

        await act.Should().ThrowAsync<SuspendedUserException>();
    }

    [Fact]
    public async Task RevokeRefreshTokens_RemovesAllRefreshTokens()
    {
        var user = await SeedUserAsync();
        _context.Tokens.AddRange(
            new Token { UserId = user.Id, TokenData = "token1", ExpireDateTime = DateTime.UtcNow.AddDays(30), TokenType = TokenType.RefreshToken },
            new Token { UserId = user.Id, TokenData = "token2", ExpireDateTime = DateTime.UtcNow.AddDays(30), TokenType = TokenType.RefreshToken },
            new Token { UserId = user.Id, TokenData = "activation", ExpireDateTime = DateTime.UtcNow.AddHours(3), TokenType = TokenType.ActivationToken });
        await _context.SaveChangesAsync();

        await _authService.RevokeRefreshTokens(user.Id, CancellationToken.None);

        var remainingTokens = await _context.Tokens.Where(t => t.UserId == user.Id).ToListAsync();
        remainingTokens.Should().ContainSingle();
        remainingTokens.First().TokenType.Should().Be(TokenType.ActivationToken);
    }

    [Fact]
    public async Task RevokeRefreshTokens_DoesNotAffectOtherUsers()
    {
        var user1 = await SeedUserAsync();
        var user2 = new User
        {
            PublicId = Guid.NewGuid(),
            PublicNick = "other",
            FirstName = "Other",
            LastName = "User",
            Email = "other@test.com",
            PasswordHash = "hash",
            RoleId = user1.RoleId
        };
        _context.Users.Add(user2);
        await _context.SaveChangesAsync();

        _context.Tokens.AddRange(
            new Token { UserId = user1.Id, TokenData = "token1", ExpireDateTime = DateTime.UtcNow.AddDays(30), TokenType = TokenType.RefreshToken },
            new Token { UserId = user2.Id, TokenData = "token2", ExpireDateTime = DateTime.UtcNow.AddDays(30), TokenType = TokenType.RefreshToken });
        await _context.SaveChangesAsync();

        await _authService.RevokeRefreshTokens(user1.Id, CancellationToken.None);

        var user2Tokens = await _context.Tokens.Where(t => t.UserId == user2.Id).ToListAsync();
        user2Tokens.Should().ContainSingle();
    }
}
