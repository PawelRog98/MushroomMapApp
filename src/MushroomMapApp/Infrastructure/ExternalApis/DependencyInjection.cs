using Microsoft.Extensions.Options;
using MushroomMapApp.Infrastructure.ExternalApis.WeatherApi;
using MushroomMapApp.Infrastructure.ExternalApis.WeatherApi.Interfaces;

namespace MushroomMapApp.Infrastructure.ExternalApis;

public static class DependencyInjection
{
    public static IServiceCollection AddExternalApis(this IServiceCollection services)
    {
        services.AddOptions<OpenMeteoOptions>()
            .BindConfiguration("OpenMeteo")
            .ValidateOnStart();

        services.AddHttpClient<IWeatherClient, OpenMeteoClient>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<OpenMeteoOptions>>();

            client.BaseAddress = new Uri(options.Value.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        return services;
    }
}
