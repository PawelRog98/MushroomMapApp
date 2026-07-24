namespace MushroomMapApp.Domain.Interfaces;

public interface IResourcePermissionEvaluator<TResource, TContext, TPermissionResult>
{
    Task<Dictionary<Guid, TPermissionResult>> Evaluate(IEnumerable<TResource> entities, TContext context, CancellationToken cancellationToken);
}
