using MushroomMapApp.Domain.Models;

namespace MushroomMapApp.Domain.Interfaces;

public interface IAuthService
{
    Task<AuthTokenModel> GenerateJwtToken(UserModel user, CancellationToken cancellationToken);
    Task<AuthTokenModel> RefreshToken(string refreshToken, CancellationToken cancellationToken);
    Task RevokeRefreshTokens(long userId, CancellationToken cancellationToken);
}
