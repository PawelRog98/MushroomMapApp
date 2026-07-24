using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using MushroomMapApp.Domain.Entities;
using MushroomMapApp.Domain.Interfaces;
using MushroomMapApp.Domain.Repositories;
using MushroomMapApp.Infrastructure.Services.Authorization;
using MushroomMapApp.Infrastructure.Services.FileStorage;
using StackExchange.Redis;

namespace MushroomMapApp.Infrastructure.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, string redisConnection)
    {
        ConnectionMultiplexer.Connect(redisConnection);

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnection;
            options.InstanceName = "AppCacheData_";
        });

        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IRedisCache, RedisCache>();
        services.AddScoped<IFileStorage, FileManager>();
        services.AddScoped<IImageProcessingService, ImageProcessingService>();
        services.AddScoped<IFilePathGenerator, FilePathGenerator>();
        services.AddScoped<IPermissionRegistry, PermissionRegistry>();
        services.AddScoped<IPermissionsSynchronizer, PermissionsSynchronizer>();
        services.AddScoped<IPermissionService, PermissionsService>();
        services.AddScoped<IPermissionsRepository, PermissionsRepository>();
        services.AddScoped<IPermissionCacheBuilder, PermissionCacheBuilder>();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

        return services;
    }
}
