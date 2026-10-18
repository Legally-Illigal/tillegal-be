using Microsoft.Extensions.DependencyInjection;
using Tillegal.Application;

namespace Tillegal.AppHost.Configurations;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection services) =>
        services.AddApplication();
}
