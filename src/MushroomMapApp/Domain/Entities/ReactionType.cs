using MushroomMapApp.Domain.Abstractions;

namespace MushroomMapApp.Domain.Entities;

public class ReactionType : ICommonData
{
    public long Id { get; set; }
    public Guid PublicId { get; set; }
    public string Key { get; set; }
    public string Name { get; set; }
    public string Icon { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
