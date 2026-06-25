namespace MushroomMapApp.Features.Reactions.GetReactionsForLocation;

public class ReactionDto
{
    public Guid PublicId { get; set; }
    public string Key { get; set; }
    public string Name { get; set; }
    public string Icon { get; set; }
    public int Count { get; set; }
}
