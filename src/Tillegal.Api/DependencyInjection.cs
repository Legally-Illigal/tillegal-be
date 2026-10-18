using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Tillegal.Api.OpenAPI;
using Tillegal.Api.Services;
using Tillegal.Application.Common.Interfaces;

namespace Tillegal.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddApi(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.ConfigureOptions<SwaggerConfigureOptions>();
        services.AddAuthorization();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        var mappingConfiguration = ApiMappingConfiguration.Create();
        services.Replace(ServiceDescriptor.Singleton(mappingConfiguration));
        services.Replace(ServiceDescriptor.Scoped<IMapper, ServiceMapper>());

        return services;
    }
}
