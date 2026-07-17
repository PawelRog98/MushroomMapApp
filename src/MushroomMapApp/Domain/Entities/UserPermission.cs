namespace MushroomMapApp.Domain.Entities;

public class UserPermission
{
    public long UserId { get; set; }

    public User User { get; set; } = null!;

    public long PermissionId { get; set; }

    public Permission Permission { get; set; } = null!;
}
