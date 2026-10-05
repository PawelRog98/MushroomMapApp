using Microsoft.Extensions.DependencyInjection;
using FluentMigrator.Runner;

namespace MushroomMap.IntegrationTests.Database;

public static class DatabaseMigrator
{
    public static void Migrate(string connectionString)
    {
        using var serviceProvider = new ServiceCollection()
            .AddFluentMigratorCore()
            .ConfigureRunner(x=>x.AddPostgres()
                .WithGlobalConnectionString(connectionString)
                .ScanIn(typeof(MushroomMapApp.Migrator.Migrations.InitializeDatabase).Assembly)
                .For.All())
            .AddLogging(x=>
                x.AddFluentMigratorConsole())
            .BuildServiceProvider(false);

        using var scope = serviceProvider.CreateScope();
        var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

        runner.MigrateUp();
    }
}
