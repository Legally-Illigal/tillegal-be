using System.Net;
using MapsterMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Tillegal.Api.OpenAPI;
using Tillegal.Api.Services;
using Tillegal.Application.Common.Interfaces;

namespace Tillegal.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddApi(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.ForwardLimit = 1;

            var knownProxies = configuration.GetSection("ReverseProxy:KnownProxies").Get<string[]>();
            if (knownProxies is null)
            {
                return;
            }

            foreach (var proxy in knownProxies)
            {
                if (!IPAddress.TryParse(proxy, out var address))
                {
                    throw new InvalidOperationException(
                        $"Invalid reverse proxy IP '{proxy}' in 'ReverseProxy:KnownProxies'. Configure trusted proxy IP addresses only.");
                }

                options.KnownProxies.Add(address);
            }
        });

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
