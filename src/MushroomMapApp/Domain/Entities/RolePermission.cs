using MushroomMapApp.Domain.Abstractions;

namespace MushroomMapApp.Domain.Entities;

public class RolePermission
{
    public long RoleId { get; set; }
    public Role Role { get; set; }
    public long PermissionId { get; set; }
    public Permission Permission { get; set; }
}
