using Microsoft.Extensions.DependencyInjection;
using Tillegal.Api;

namespace Tillegal.AppHost.Configurations;

public static class ApiExtensions
{
    public static IServiceCollection AddApiLayer(this IServiceCollection services) =>
        services.AddApi();
}
