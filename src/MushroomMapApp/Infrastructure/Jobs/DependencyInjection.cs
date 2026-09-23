using Hangfire;
using Hangfire.PostgreSql;
using MushroomMapApp.Features.Jobs.Abstraction;
using Npgsql;
using System.Reflection;
using MushroomMapApp.Features.Jobs.Triggered;
using MushroomMapApp.Features.Jobs.Triggered.Interfaces;

namespace MushroomMapApp.Infrastructure.Jobs;

public static class DependencyInjection
{
    public static IServiceCollection AddBackgroundJobs(this IServiceCollection services, string connectionString)
    {
        var jobTypes = typeof(IRecurringJob).Assembly
            .GetTypes()
            .Where(t =>
                typeof(IRecurringJob).IsAssignableFrom(t) &&
                !t.IsAbstract &&
                !t.IsInterface &&
                t.GetCustomAttribute<RecurringJobAttribute>() != null);

        foreach (var type in jobTypes)
        {
            services.AddScoped(typeof(IRecurringJob), type);
        }

        services.AddScoped<JobRegistrar>();

        services.AddHangfire(conf =>
            conf.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(options =>
                    options.UseNpgsqlConnection(connectionString), new PostgreSqlStorageOptions
                    {
                        QueuePollInterval = TimeSpan.FromSeconds(5),
                        PrepareSchemaIfNecessary = true,
                        SchemaName = "hangfire",
                        InvisibilityTimeout = TimeSpan.FromMinutes(5),
                        DistributedLockTimeout = TimeSpan.FromMinutes(10),
                        UseNativeDatabaseTransactions = true
                    }));

        services.AddHangfireServer(options =>
        {
            options.WorkerCount = Environment.ProcessorCount * 5;
            options.ServerName = "MushroomMapApp_Server";
        });

        return services;
    }

    public static IServiceCollection AddJobs(this IServiceCollection services)
    {
        services.AddScoped<ISendVerificationJob, SendVerificationCodeJob>();

        return services;
    }
}
