namespace MushroomMapApp.Features.Users.Login;

public class AuthTokenDto
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public string UserNick { get; set; }
    public string UserId { get; set; }
}