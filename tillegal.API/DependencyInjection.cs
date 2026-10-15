using System.Reflection;

using Mapster;

using Tillegal.API.Common;

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
            TypeAdapterConfig config = TypeAdapterConfig.GlobalSettings;
            config.Scan(Assembly.GetExecutingAssembly());

            return services;
        }
    }
}
