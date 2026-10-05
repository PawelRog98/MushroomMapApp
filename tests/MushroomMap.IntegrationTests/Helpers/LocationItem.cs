using MushroomMapApp.Features.Locations.GetLocations;

namespace MushroomMap.IntegrationTests.Helpers;

public sealed class LocationItem
{
    public LocationListItemDto Data { get; set; } = new();
    public LocationItemMeta Meta { get; set; } = new();
}

public sealed class LocationItemMeta
{
    public bool CanView { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
}
