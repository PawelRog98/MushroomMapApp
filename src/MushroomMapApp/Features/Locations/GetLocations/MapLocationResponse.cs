using MushroomMapApp.Shared.Response;

namespace MushroomMapApp.Features.Locations.GetLocations;

public class MapLocationsResponse
{
    public string Mode { get; set; } = "markers";
    public int TotalCount { get; set; }
    public bool Truncated { get; set; }
    public List<ItemWithMeta<LocationListItemDto>> Markers { get; set; } = new();
    public List<LocationClusterDto> Clusters { get; set; } = new();
}
