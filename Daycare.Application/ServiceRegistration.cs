using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Daycare.Application;

/// <summary>
/// Dependency injection configuration for Application layer.
/// Registers MediatR, validators, and other application services.
/// </summary>
public static class ServiceRegistration
{
    /// <summary>
    /// Adds Application services to the dependency injection container.
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register MediatR
        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(typeof(ServiceRegistration).Assembly);
        });

        // Register FluentValidation
        services.AddValidatorsFromAssembly(typeof(ServiceRegistration).Assembly);

        return services;
    }
}
