using MushroomMapApp.Domain.Abstractions;

namespace MushroomMapApp.Domain.Entities;

public class Permission : ICommonData
{
    public long Id { get; set; }
    public Guid PublicId { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public bool IsActive { get; set; } = true;
}
