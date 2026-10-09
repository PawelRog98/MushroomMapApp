using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MushroomMapApp.Domain.Interfaces;
using MushroomMapApp.Infrastructure.ExternalApis.WeatherApi.Interfaces;

namespace MushroomMap.IntegrationTests.Fixtures;

public class MushroomMapApplicationFactory : WebApplicationFactory<Program>
{
    public FakeEmailService EmailService { get; } = new();
    public FakeWeatherClient WeatherClient { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("IntegrationTests");
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IEmailService>();
            services.AddSingleton<IEmailService>(EmailService);

            services.RemoveAll<IWeatherClient>();
            services.AddSingleton<IWeatherClient>(WeatherClient);
        });
    }
}
