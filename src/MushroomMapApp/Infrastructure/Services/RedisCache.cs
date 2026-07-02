using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using MushroomMapApp.Domain.Interfaces;

namespace MushroomMapApp.Infrastructure.Services;

public class RedisCache : IRedisCache
{
    private readonly IDistributedCache _distributedCache;

    public RedisCache(IDistributedCache distributedCache)
    {
        _distributedCache = distributedCache;
    }


    public async Task<T?> GetAsync<T>(string key, CancellationToken token = default)
    {
        var jsonData = await _distributedCache.GetStringAsync(key);

        if(jsonData == null)
            return default;

        return JsonSerializer.Deserialize<T>(jsonData);
    }

    public async Task SetAsync<T>(string key, T data, TimeSpan? timespan = null, CancellationToken token = default)
    {
        if(data == null)
            return;

        var jsonData = JsonSerializer.Serialize(data);
        var expiration = timespan ?? TimeSpan.FromMinutes(5);

        await _distributedCache.SetStringAsync(key, jsonData, options: new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration
        });
    }
}
