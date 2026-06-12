using MushroomMapApp.Domain.Abstractions;

namespace MushroomMapApp.Domain.Entities;

public class Role : ICommonData
{
    public long Id { get; set; }
    public Guid PublicId { get; set; }
    public string Name { get; set; } = string.Empty;
}
