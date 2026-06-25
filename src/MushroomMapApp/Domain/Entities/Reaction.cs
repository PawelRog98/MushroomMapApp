using System.ComponentModel.DataAnnotations.Schema;
using MushroomMapApp.Domain.Enums;

namespace MushroomMapApp.Domain.Entities;

public class Reaction
{
    public long LocationId  { get; set; }
    public Location Location { get; set; }
    public long UserId { get; set; }
    public User User { get; set; }
    public long ReactionTypeId { get; set; }
    public ReactionType ReactionType { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
