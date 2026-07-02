using FluentValidation;
using MediatR;
using MushroomMapApp.Features.Common.Behaviors;

namespace MushroomMapApp.Features.Common;

public static class DependencyInjection
{
    public static IServiceCollection AddCommonFeatures(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}
