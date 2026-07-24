using MushroomMapApp.Domain.Entities;
using MushroomMapApp.Domain.Interfaces;
using MushroomMapApp.Domain.Permissions;

namespace MushroomMapApp.Features.Locations.GetLocations;

public class LocationPermissionResult
{
    public bool CanView { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
}

public class LocationPermissionContext
{
    public long UserId { get; set; }
    public IReadOnlySet<string> Permissions { get; set; }
}

public class LocationPermissionEvaluator : IResourcePermissionEvaluator<Location, LocationPermissionContext, LocationPermissionResult>
{
    public Task<Dictionary<Guid, LocationPermissionResult>> Evaluate(IEnumerable<Location> locations, LocationPermissionContext context, CancellationToken cancellationToken)
    {
        var result = new Dictionary<Guid, LocationPermissionResult>();

        var hasEdit = context.Permissions.Contains(Permissions.Locations.Edit.Code);
        var hasDelete = context.Permissions.Contains(Permissions.Locations.Delete.Code);

        foreach (var location in locations)
        {
            var isOwner = location.CreatedById ==  context.UserId;
            result[location.PublicId] = new LocationPermissionResult
            {
                CanView = true,
                CanEdit = hasEdit || isOwner,
                CanDelete = hasDelete || isOwner
            };
        }

        return Task.FromResult(result);
    }
}
