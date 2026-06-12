using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using DotNetEnv;

namespace MushroomMapApp.Migrator;

public class Program
{
    static void Main(string[] args)
    {
        Env.Load();
        
        var dbConnString = args.Length > 0 ? args[0] : Environment.GetEnvironmentVariable("SQL_CONN");
        var operation = args.Length > 1 ? args[1].ToLower() : "migrate";
        var migrationsQuantity = args.Length > 2 ? Int32.Parse(args[2]) : 1;

        if (string.IsNullOrEmpty(dbConnString))
        {
            Console.WriteLine("Can't find sql connection string. Ensure SQL_CONN is set in .env or passed as argument.");
            return;
        }

        var serviceProvider = CreateServices(dbConnString);

        using (var scope = serviceProvider.CreateScope())
        {
            UpdateDatabase(scope.ServiceProvider, operation, migrationsQuantity);
        }
    }
    
    private static IServiceProvider CreateServices(string dbConnectionString)
    {
        return new ServiceCollection()
            .AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                .AddPostgres()
                .WithGlobalConnectionString(dbConnectionString)
                .ScanIn(typeof(Program).Assembly).For.Migrations())
            .AddLogging(lb => lb.AddFluentMigratorConsole())
            .BuildServiceProvider(false);
    }

    private static void UpdateDatabase(IServiceProvider serviceProvider, string operation, int quantity = 0)
    {
        var runner = serviceProvider.GetRequiredService<IMigrationRunner>();

        switch (operation)
        {
            case "migrate":
                runner.MigrateUp();
                break;
            case "rollback":
                runner.Rollback(quantity);
                break;
            case "rollbackall":
                runner.RollbackToVersion(0);
                break;
            default:
                Console.WriteLine("Use 'migrate', 'rollback' or 'rollbackall' command");
                break;
        }
    }
}
