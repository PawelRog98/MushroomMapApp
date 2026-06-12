using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using MushroomMapApp.Domain.Data;
using MushroomMapApp.Domain.Entities;
using MushroomMapApp.Domain.Enums;
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
}
