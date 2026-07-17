namespace MushroomMapApp.Domain.Interfaces;

public interface IPermissionsRepository
{
    Task<IReadOnlyCollection<string>> GetAllPermissionsForUser(long userId,
        CancellationToken cancellationToken);
}
