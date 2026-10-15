using Tillegal.API.Common;
using Mapster;
using System.Reflection;

namespace Tillegal.API
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPresentation(this IServiceCollection services)
        {
            services.AddControllers(options =>
            {
                options.Filters.Add<ApiResponseWrapperFilter>();
            });

            services.AddExceptionHandler<GlobalExceptionHandler>();
            services.AddProblemDetails();
            var config = TypeAdapterConfig.GlobalSettings;
            config.Scan(Assembly.GetExecutingAssembly());

            return services;
        }
    }
}