using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using MushroomMapApp.Domain.Data;
using MushroomMap.IntegrationTests.Database;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using Xunit;

namespace MushroomMap.IntegrationTests.Fixtures;

public class IntegrationTestFixture : IAsyncLifetime
{
    private const string TestJwtKey =
        "integration-tests-jwt-signing-key-with-at-least-32-bytes-0123456789abcdef";

    private readonly PostgreSqlContainer _postgres;
    private readonly RedisContainer _redis;
    private readonly string _storageRoot;
    private MushroomMapApplicationFactory? _factory;

    public IntegrationTestFixture()
    {
        _postgres = new PostgreSqlBuilder("postgis/postgis:17-3.5")
            .WithDatabase("MushroomMap_Test")
            .WithUsername("MushroomMap_User")
            .WithPassword("MushroomMap_Password")
            .Build();

        _redis = new RedisBuilder("redis:8.4")
            .Build();

        _storageRoot = Path.Combine(Path.GetTempPath(), $"mushroommap-tests-{Guid.NewGuid():N}");
    }

    public string PostgresConnectionString => _postgres.GetConnectionString();

    public string RedisConnectionString => _redis.GetConnectionString();

    public string StorageRoot => _storageRoot;

    public MushroomMapApplicationFactory Factory =>
        _factory ?? throw new InvalidOperationException("IntegrationTestFixture has not been initialized.");

    public HttpClient CreateClient() => Factory.CreateClient();

    public async Task<T> QueryDbAsync<T>(Func<AppDbContext, Task<T>> query)
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await query(dbContext);
    }

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        await _redis.StartAsync();

        DatabaseMigrator.Migrate(PostgresConnectionString);

        Directory.CreateDirectory(_storageRoot);

        var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") ?? TestJwtKey;
        Environment.SetEnvironmentVariable("JWT_KEY", jwtKey);

        ApplyTestConfiguration();

        _factory = new MushroomMapApplicationFactory();

        using var scope = Factory.Services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<DbSeeder>();
        await seeder.SeedAsync();
    }

    public async Task DisposeAsync()
    {
        if (_factory is not null)
        {
            await _factory.DisposeAsync();
        }

        await _postgres.DisposeAsync();
        await _redis.DisposeAsync();

        if (Directory.Exists(_storageRoot))
        {
            Directory.Delete(_storageRoot, recursive: true);
        }
    }

    private void ApplyTestConfiguration()
    {
        Environment.SetEnvironmentVariable("SQL_CONN", PostgresConnectionString);
        Environment.SetEnvironmentVariable("REDIS_CONN", RedisConnectionString);
        Environment.SetEnvironmentVariable("API_URL", "http://localhost");
        Environment.SetEnvironmentVariable("JWT_KEY", TestJwtKey);
        Environment.SetEnvironmentVariable("JWT_ISSUER", "MushroomMapApp.IntegrationTests");
        Environment.SetEnvironmentVariable("JWT_TIME_EXPIRE_MINUTES", "60");
        Environment.SetEnvironmentVariable("FILES_STORAGE", _storageRoot);
        Environment.SetEnvironmentVariable("EMAIL_HOST", "smtp.tests.local");
        Environment.SetEnvironmentVariable("EMAIL_PORT", "587");
        Environment.SetEnvironmentVariable("EMAIL", "noreply@tests.local");
        Environment.SetEnvironmentVariable("EMAIL_USERNAME", "tests");
        Environment.SetEnvironmentVariable("EMAIL_PASSWORD", "tests");
        Environment.SetEnvironmentVariable("EMAIL_FROMNAME", "MushroomMapTests");
    }
}
