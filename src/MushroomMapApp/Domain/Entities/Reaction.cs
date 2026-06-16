using System.ComponentModel.DataAnnotations.Schema;
using MushroomMapApp.Domain.Enums;

namespace MushroomMapApp.Domain.Entities;

public class Reaction
{
    public long LocationId  { get; set; }
    public Location Location { get; set; }
    public long UserId { get; set; }
    public User User { get; set; }
    public string ReactionTypeValue { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    [NotMapped]
    public ReactionEnum ReactionType
    {
        get => Enum.TryParse<ReactionEnum>(ReactionTypeValue, out var result) ? result : default;
        set => ReactionTypeValue = value.ToString();
    }
}
