namespace MushroomMapApp.Domain.Interfaces;

public interface IPermissionCacheBuilder
{
    Task<HashSet<string>> Build(long userId, CancellationToken cancellationToken);
}
