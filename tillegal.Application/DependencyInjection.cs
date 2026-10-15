using FluentValidation;
using Mapster;
using MapsterMapper;
using Tillegal.Application.Common.Behaviors;
using Tillegal.Application.Authentication;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Tillegal.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(
            this IServiceCollection services)
        {
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(
                    Assembly.GetExecutingAssembly());
                cfg.AddOpenBehavior(
                    typeof(ValidationBehavior<,>));
            });
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            services.AddScoped<IAuthenticationService, AuthenticationService>();

            var config = TypeAdapterConfig.GlobalSettings;
            config.Scan(Assembly.GetExecutingAssembly());

            services.AddSingleton(config);
            services.AddScoped<IMapper, Mapper>();

            return services;
        }
    }
}