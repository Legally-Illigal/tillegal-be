using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tillegal.Api;

namespace Tillegal.AppHost.Configurations;

public static class ApiExtensions
{
    public static IServiceCollection AddApiLayer(
        this IServiceCollection services,
        IConfiguration configuration) =>
        services.AddApi(configuration);
}
