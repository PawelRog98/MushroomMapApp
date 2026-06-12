using MushroomMapApp.Domain.Models;

namespace MushroomMapApp.Domain.Interfaces;

public interface IAuthService
{
    Task<AuthTokenModel> GenerateJwtToken(UserModel user, CancellationToken cancellationToken);
}
