namespace MushroomMapApp.Domain.Interfaces;

public interface IPermissionsContextFactory<TContext>
{
    Task<TContext> Create(CancellationToken cancellationToken);
}
