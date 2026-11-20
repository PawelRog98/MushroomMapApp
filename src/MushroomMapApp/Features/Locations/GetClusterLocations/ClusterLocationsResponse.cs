using MushroomMapApp.Features.Locations.GetLocations;

namespace MushroomMapApp.Features.Locations.GetClusterLocations;

public record ClusterLocationsResponse(
    IReadOnlyList<LocationListItemDto> Items,
    Dictionary<Guid, LocationPermissionResult> Permissions,
    int TotalCount,
    int Page,
    int PageSize);
