using Microsoft.Extensions.DependencyInjection;
using MushroomMapApp.Domain.Data;
using MushroomMap.IntegrationTests.Database;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using Xunit;

namespace MushroomMap.IntegrationTests.Fixtures;

public sealed class IntegrationTestFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres;
    private readonly RedisContainer _redis;

    private MushroomMapApplicationFactory? _factory;

    public IntegrationTestFixture()
    {
        DockerUtilities.EnsureDockerAvailable();

        _postgres = new PostgreSqlBuilder("postgis/postgis:17-3.5")
            .WithDatabase("MushroomMap_Test")
            .WithUsername("MushroomMap_User")
            .WithPassword("MushroomMap_Password")
            .Build();

        _redis = new RedisBuilder("redis:8.4").Build();
    }

    public string PostgresConnectionString => _postgres.GetConnectionString();
    public string RedisConnectionString => _redis.GetConnectionString();
    public string StorageRoot => TestConfiguration.StorageRoot;

    public MushroomMapApplicationFactory Factory =>
        _factory ?? throw new InvalidOperationException("Fixture not initialized. Call InitializeAsync() first.");

    public HttpClient CreateClient() => Factory.CreateClient();

    public async Task<T> QueryDbAsync<T>(Func<AppDbContext, Task<T>> query)
    {
        using var scope = Factory.Services.CreateScope();
        return await query(scope.ServiceProvider.GetRequiredService<AppDbContext>());
    }

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        await _redis.StartAsync();

        TestConfiguration.Apply(PostgresConnectionString, RedisConnectionString);

        Directory.CreateDirectory(TestConfiguration.StorageRoot);
        DatabaseMigrator.Migrate(PostgresConnectionString);

        _factory = new MushroomMapApplicationFactory();

        await using var scope = Factory.Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<DbSeeder>().SeedAsync();
    }

    public async Task DisposeAsync()
    {
        if (_factory is not null)
        {
            await _factory.DisposeAsync();
            _factory = null;
        }

        await _postgres.DisposeAsync();
        await _redis.DisposeAsync();

        if (Directory.Exists(TestConfiguration.StorageRoot))
            Directory.Delete(TestConfiguration.StorageRoot, recursive: true);
    }
}
