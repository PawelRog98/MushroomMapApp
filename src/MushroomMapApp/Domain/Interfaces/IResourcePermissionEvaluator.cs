namespace MushroomMapApp.Domain.Interfaces;

public interface IResourcePermissionEvaluator<TResource>
{
    Task<dynamic> Evaluate(TResource resource, CancellationToken cancellationToken);
}
