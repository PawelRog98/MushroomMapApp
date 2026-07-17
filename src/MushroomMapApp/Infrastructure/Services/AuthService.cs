using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MushroomMapApp.Domain.Data;
using MushroomMapApp.Domain.Entities;
using MushroomMapApp.Domain.Enums;
using MushroomMapApp.Domain.Exceptions;
using MushroomMapApp.Domain.Interfaces;
using MushroomMapApp.Domain.Models;

namespace MushroomMapApp.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly JwtSettings _jwtSettings;

    public AuthService(AppDbContext context, JwtSettings jwtSettings)
    {
        _context = context;
        _jwtSettings = jwtSettings;
    }

    public async Task<AuthTokenModel> GenerateJwtToken(UserModel user, CancellationToken cancellationToken)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new Claim(ClaimTypes.Role, user.RoleName),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.JwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(_jwtSettings.JwtExpireMinutes);

        var token = new JwtSecurityToken(
            _jwtSettings.JwtIssuer,
            _jwtSettings.JwtIssuer,
            claims,
            expires: expires,
            signingCredentials: credentials);

        var tokenHandler = new JwtSecurityTokenHandler();
        var accessToken = tokenHandler.WriteToken(token);

        var refreshToken = new Token
        {
            UserId = user.Id,
            TokenData = Guid.NewGuid().ToString(),
            ExpireDateTime = DateTime.UtcNow.AddDays(30),
            TokenType = TokenType.RefreshToken
        };

        _context.Tokens.Add(refreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new AuthTokenModel(accessToken, refreshToken.TokenData, user.PublicNick);
    }

    public async Task<AuthTokenModel> RefreshToken(string refreshToken, CancellationToken cancellationToken)
    {
        var storedToken = await _context.Tokens
            .FirstOrDefaultAsync(t => t.TokenData == refreshToken, cancellationToken);

        if (storedToken is null || storedToken.TokenType != TokenType.RefreshToken)
            throw new BadRequestException("Invalid refresh token.");

        if (storedToken.ExpireDateTime < DateTime.UtcNow)
            throw new BadRequestException("Refresh token has expired.");

        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == storedToken.UserId, cancellationToken);

        if (user is null)
            throw new BadRequestException("User not found.");

        _context.Tokens.Remove(storedToken);

        var userModel = new UserModel(user.Id, user.FirstName, user.LastName, user.Role.Name, user.PublicNick);
        var result = await GenerateJwtToken(userModel, cancellationToken);

        return result;
    }

    public async Task RevokeRefreshTokens(long userId, CancellationToken cancellationToken)
    {
        var refreshTokens = await _context.Tokens
            .Where(t => t.UserId == userId && t.TokenType == TokenType.RefreshToken)
            .ToListAsync(cancellationToken);

        _context.Tokens.RemoveRange(refreshTokens);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
