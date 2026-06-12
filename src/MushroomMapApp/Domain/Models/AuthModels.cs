namespace MushroomMapApp.Domain.Models;

public class JwtSettings
{
    public string JwtKey { get; set; } = string.Empty;
    public int JwtExpireMinutes { get; set; }
    public string JwtIssuer { get; set; } = string.Empty;
}

public record UserModel(long Id, string FirstName, string LastName, string RoleName, string PublicNick);

public record AuthTokenModel(string AccessToken, string RefreshToken, string UserNick);
