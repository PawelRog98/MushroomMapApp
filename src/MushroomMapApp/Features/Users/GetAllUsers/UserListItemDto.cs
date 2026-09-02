namespace MushroomMapApp.Features.Users.GetAllUsers;

public class UserListItemDto
{
    public Guid PublicId { get; set; }
    public string PublicNick { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public bool IsActiveSuspension { get; set; }
    public DateTime? SuspensionEndDate { get; set; }
    public List<string> ActivePermissions { get; set; } = [];
}
