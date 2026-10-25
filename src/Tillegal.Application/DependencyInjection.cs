using FluentValidation;
using MapsterMapper;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Tillegal.Application.Common.Behaviors;
using Tillegal.Application.Common.Mappings;
using Tillegal.Domain.Services;

namespace Tillegal.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var applicationAssembly = typeof(DependencyInjection).Assembly;
        var mappingConfiguration = MappingConfiguration.Create();

        services.AddSingleton(mappingConfiguration);
        services.AddScoped<IMapper, ServiceMapper>();
        services.AddScoped<AuthService>();
        services.AddScoped<ConversationService>();
        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(applicationAssembly);
            configuration.AddOpenBehavior(typeof(LoggingBehavior<,>));
            configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });
        services.AddValidatorsFromAssembly(applicationAssembly);

        return services;
    }
}
