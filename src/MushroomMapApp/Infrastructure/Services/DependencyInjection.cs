using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MushroomMapApp.Domain.Entities;
using MushroomMapApp.Domain.Interfaces;
using MushroomMapApp.Domain.Models;
using MushroomMapApp.Domain.Repositories;
using MushroomMapApp.Infrastructure.Services.Authorization;
using MushroomMapApp.Infrastructure.Services.Email.Smtp;
using MushroomMapApp.Infrastructure.Services.FileStorage;
using StackExchange.Redis;

namespace MushroomMapApp.Infrastructure.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, string redisConnection, IConfiguration configuration)
    {
        ConnectionMultiplexer.Connect(redisConnection);

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnection;
            options.InstanceName = "AppCacheData_";
        });

        var emailSettings = new EmailSettings();
        configuration.GetSection("EmailConfiguration").Bind(emailSettings);
        services.AddSingleton(emailSettings);

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
        services.AddScoped<IEmailService, SmtpEmailService>();

        return services;
    }
}
