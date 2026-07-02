namespace MushroomMapApp.Domain.Interfaces;

public interface IRedisCache
{
    Task<T?>  GetAsync<T>(string key, CancellationToken token = default);
    Task SetAsync<T>(string key, T value, TimeSpan? timespan, CancellationToken token = default);
}
