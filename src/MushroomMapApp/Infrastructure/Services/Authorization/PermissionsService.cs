using MushroomMapApp.Domain.Interfaces;

namespace MushroomMapApp.Infrastructure.Services.Authorization;

public class PermissionsService : IPermissionService
{
    private readonly IRedisCache _redisCache;
    private readonly IPermissionCacheBuilder _permissionCacheBuilder;

    public PermissionsService(IPermissionCacheBuilder permissionCacheBuilder, IRedisCache redisCache)
    {
        _permissionCacheBuilder = permissionCacheBuilder;
        _redisCache = redisCache;
    }

    public async Task<IReadOnlySet<string>> GetPermissions(long userId, CancellationToken cancellationToken)
    {
        var key = CacheKey(userId);

        var cached = await _redisCache.GetAsync<HashSet<string>>(key, cancellationToken);

        if (cached != null)
            return cached;

        var permissions = await _permissionCacheBuilder.Build(userId, cancellationToken);

        await _redisCache.SetAsync(key, permissions, TimeSpan.FromMinutes(30), cancellationToken);

        return permissions;
    }

    public async Task<bool> HasPermission(long userId, string permissionName, CancellationToken cancellationToken)
    {
        var permissions = await GetPermissions(userId, cancellationToken);
        return permissions.Contains(permissionName);
    }

    private static string CacheKey(long userId)
        => $"permissions:{userId}";
}
