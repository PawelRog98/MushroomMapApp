namespace MushroomMapApp.Features.Users.GetUserData;

public class UserDataDto
{
    public Guid UserId { get; set; }
    public string PublicNick { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string AccountInfo  { get; set; }
    public bool IsEmailConfirmed { get; set; }
    public string RoleName  { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
