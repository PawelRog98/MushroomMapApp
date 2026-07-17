namespace MushroomMapApp.Domain.Interfaces;

public interface IPermissionsSynchronizer
{
    Task Synchronize(CancellationToken cancellationToken);
}
