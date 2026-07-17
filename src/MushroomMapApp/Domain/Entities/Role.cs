using MushroomMapApp.Domain.Abstractions;

namespace MushroomMapApp.Domain.Entities;

public class Role : ICommonData
{
    public Role()
    {
        Permissions = new HashSet<RolePermission>();
    }

    public long Id { get; set; }
    public Guid PublicId { get; set; }
    public string Name { get; set; } = string.Empty;
    public ICollection<RolePermission> Permissions { get; set; }
}
