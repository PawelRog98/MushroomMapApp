using Hangfire;
using Hangfire.PostgreSql;
using Npgsql;

namespace MushroomMapApp.Infrastructure.Jobs;

public static class DependencyInjection
{
    public static IServiceCollection AddBackgroundJobs(this IServiceCollection services, string connectionString)
    {
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
}
