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

public sealed class MapLocationsPayload
{
    public string Mode { get; set; } = string.Empty;
    public int TotalCount { get; set; }
    public bool Truncated { get; set; }
    public List<LocationItem> Markers { get; set; } = new();
    public List<LocationClusterDto> Clusters { get; set; } = new();
}

public sealed class ClusterLocationsPage
{
    public List<LocationItem> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasPrevious { get; set; }
    public bool HasNext { get; set; }
}
