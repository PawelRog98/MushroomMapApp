using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MushroomMap.IntegrationTests.Fixtures;
using MushroomMapApp.Domain.Interfaces;
using Xunit;

namespace MushroomMap.IntegrationTests;

public class SmokeTests : IntegrationTestBase
{
    public SmokeTests(IntegrationTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public void Environment_is_integration_tests()
    {
        var environment = Fixture.Factory.Services.GetRequiredService<IWebHostEnvironment>();

        environment.EnvironmentName.Should().Be("IntegrationTests");
    }

    [Fact]
    public void Configuration_points_to_test_containers_and_temp_storage()
    {
        var configuration = Fixture.Factory.Services.GetRequiredService<IConfiguration>();

        configuration.GetConnectionString("DefaultConnection").Should().Be(Fixture.PostgresConnectionString);
        configuration.GetConnectionString("RedisConnection").Should().Be(Fixture.RedisConnectionString);
        configuration["FileStorage:RootPath"].Should().StartWith(Fixture.StorageRoot);
        configuration["JWTAuth:JwtIssuer"].Should().Be("MushroomMapApp.IntegrationTests");
    }

    [Fact]
    public void Email_service_is_replaced_by_fake()
    {
        var emailService = Fixture.Factory.Services.GetRequiredService<IEmailService>();

        emailService.Should().BeOfType<FakeEmailService>();
    }
}
