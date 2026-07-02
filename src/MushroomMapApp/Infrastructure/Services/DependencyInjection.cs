using Microsoft.AspNetCore.Identity;
using MushroomMapApp.Domain.Entities;
using MushroomMapApp.Domain.Interfaces;
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

        return services;
    }
}
