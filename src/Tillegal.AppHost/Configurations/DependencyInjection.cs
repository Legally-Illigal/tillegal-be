using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Tillegal.AppHost.Configurations;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddApplicationLayer();
        services.AddApiLayer();
        services.AddDatabaseLayer(configuration);
        services.AddInfrastructureLayer(configuration);
        services.AddHealthCheckServices();

        return services;
    }
}
