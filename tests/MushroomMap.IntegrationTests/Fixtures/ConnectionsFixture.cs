using Testcontainers.PostgreSql;
using Xunit;

namespace MushroomMap.IntegrationTests.Fixtures;

public class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container;

    public string ConnectionString => _container.GetConnectionString();

    public PostgresFixture()
    {
        _container = new PostgreSqlBuilder()
            .WithImage("postgis/postgis:17-5.1")
            .WithDatabase("MushroomMap_Test")
            .WithUsername("MushroomMap_User")
            .WithPassword("MushroomMap_Password")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
    }

    public async  Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }

}
